using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubeDataApiWrapper.Portable.RequestBuilders;
using Famoser.YoutubeDataApiWrapper.Portable.RequestServices;
using Famoser.YoutubeDataApiWrapper.Portable.Util;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IVideoRespository _videoRespository;
        private readonly ISmartRepository _smartRepository;
        private readonly IPlatformService _platformService;

        public PlaylistRepository(ISettingsRepository settingsRepository, IPlatformService platformService, IVideoRespository videoRespository, ISmartRepository smartRepository)
        {
            _settingsRepository = settingsRepository;
            _platformService = platformService;
            _videoRespository = videoRespository;
            _smartRepository = smartRepository;
        }

        private ObservableCollection<PlaylistModel> _playlists;
        public async Task<ObservableCollection<PlaylistModel>> GetPlaylists()
        {
            try
            {
                if (_playlists == null)
                {
                    _playlists = new ObservableCollection<PlaylistModel>();
                    var cache = await _settingsRepository.GetCache();
                    if (cache != null)
                    {
                        var converter = new PlaylistConverter();
                        foreach (var playlist in cache.CachedPlaylists)
                        {
                            _playlists.Add(converter.Convert(playlist));
                        }
                    }
                    BacklinkList();
                }
                return _playlists;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
                _playlists = new ObservableCollection<PlaylistModel>();
            }
            return _playlists;
        }

        private async Task<bool> RefreshPlaylistWorker(PlaylistModel playlist, IProgressService progressService)
        {
            try
            {
                progressService.StartIndeterminateProgress(IndeterminateProgressKeys.RefreshPlaylist);
                var youtubeService = await GetService();

                if (youtubeService != null)
                {
                    var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService, "snippet, contentDetails")
                    {
                        PlaylistId = playlist.Id
                    };
                    var playlistItemsRequestService =
                        new YoutubeListRequestService
                            <PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                            (plistItemsListRequestBuilder);

                    var obj = (await playlistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000))).ToList();

                    for (int index = 0; index < obj.Count; index++)
                    {
                        var playlistItem = obj.ToList()[index];
                        var video = playlist.Videos.FirstOrDefault(v => v.Id == playlistItem.ContentDetails.VideoId);

                        if (video != null)
                        {
                            playlist.Videos.Remove(video);
                            video.Name = playlistItem.Snippet.Title;
                        }
                        else
                        {
                            video = new VideoModel
                            {
                                Id = playlistItem.ContentDetails.VideoId,
                                Name = playlistItem.Snippet.Title,
                                PlaylistModel = playlist
                            };
                        }
                        if (playlist.Videos.Count < index)
                            playlist.Videos.Insert(index, video);
                        else
                            playlist.Videos.Add(video);
                    }

                    progressService.StopIndeterminateProgress(IndeterminateProgressKeys.RefreshPlaylist);
                    return true;
                }

            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            progressService.StopIndeterminateProgress(IndeterminateProgressKeys.RefreshPlaylist);
            return false;
        }

        public async Task<bool> RefreshPlaylist(PlaylistModel playlist, IProgressService progressService)
        {
            try
            {
                playlist.ProgressService = new ProgressService();
                if (await RefreshPlaylistWorker(playlist, playlist.ProgressService))
                    return await _settingsRepository.SaveCache();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<bool> DownloadVideosForPlaylist(PlaylistModel playlist, IProgressService progressService)
        {
            try
            {
                var tot = playlist.Videos.Count(w => w.SaveStatus < SaveStatus.Finished);
                progressService.ConfigurePercentageProgress(tot);
                
                await DownloadVideosForPlaylistWorker(playlist, progressService);

                await _settingsRepository.SaveCache();
                progressService.HidePercentageProgress();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        private void BacklinkList()
        {
            foreach (var playlistModel in _playlists)
            {
                foreach (var videoModel in playlistModel.Videos)
                {
                    videoModel.Mp3Model.VideoModel = videoModel;
                    videoModel.PlaylistModel = playlistModel;
                }
            }
        }

        public async Task<bool> RefreshAllPlaylists(IProgressService progressService)
        {
            try
            {
                progressService.StartIndeterminateProgress(IndeterminateProgressKeys.GatheringPlaylists);
                var youtubeService = await GetService();

                if (youtubeService != null)
                {
                    var channelrequestTask = youtubeService.Channels.List("id");
                    channelrequestTask.Mine = true;

                    var channelrequest = await channelrequestTask.ExecuteAsync();
                    var channelId = channelrequest.Items[0].Id;

                    var playlistrequestTask = youtubeService.Playlists.List("snippet,contentDetails");
                    playlistrequestTask.ChannelId = channelId;
                    var rawPlaylists = new List<Playlist>();

                    var response = await playlistrequestTask.ExecuteAsync();
                    rawPlaylists.AddRange(response.Items);
                    while (response.NextPageToken != null)
                    {
                        playlistrequestTask.PageToken = response.NextPageToken;
                        response = await playlistrequestTask.ExecuteAsync();
                        rawPlaylists.AddRange(response.Items);
                    }

                    foreach (var rawPlaylist in rawPlaylists)
                    {
                        PlaylistModel playlist;
                        if (_playlists.All(l => l.Id != rawPlaylist.Id))
                        {
                            playlist = new PlaylistModel()
                            {
                                Id = rawPlaylist.Id,
                                Name = rawPlaylist.Snippet.Title,
                            };
                            InsertInOrder(playlist);
                        }
                        else
                        {
                            playlist = _playlists.FirstOrDefault(l => l.Id == rawPlaylist.Id);
                        }

                        playlist.ProgressService = new ProgressService();
                        await RefreshPlaylistWorker(playlist, playlist.ProgressService);
                    }

                    await _settingsRepository.SaveCache();

                    progressService.StopIndeterminateProgress(IndeterminateProgressKeys.GatheringPlaylists);
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            progressService.StopIndeterminateProgress(IndeterminateProgressKeys.GatheringPlaylists);
            return false;
        }

        public async Task<bool> DownloadVideosForAllPlaylists(IProgressService progressService)
        {
            try
            {
                var tot = _playlists.Where(p => p.Refresh).Sum(l => l.Videos.Count(w => w.SaveStatus < SaveStatus.Finished));
                progressService.ConfigurePercentageProgress(tot);
                foreach (var playlist in _playlists.Where(p => p.Refresh))
                {
                    await DownloadVideosForPlaylistWorker(playlist, progressService);
                }

                await _settingsRepository.SaveCache();

                progressService.HidePercentageProgress();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        private async Task<bool> DownloadVideosForPlaylistWorker(PlaylistModel playlist,
            IProgressService progressService)
        {
            try
            {
                foreach (var videoModel in playlist.Videos)
                {
                    if (videoModel.SaveStatus < SaveStatus.Finished)
                    {
                        videoModel.ProgressService = new ProgressService();
                        var stream = await DownloadHelper.DownloadYoutubeVideo(videoModel, videoModel.ProgressService);
                        if (stream != null &&
                            await _videoRespository.CreateToMusicLibrary(videoModel, stream) &&
                            await _smartRepository.FillAutomaticProperties(videoModel.Mp3Model))
                        {
                            videoModel.SaveStatus = SaveStatus.Finished;

                            await _videoRespository.SaveToMusicLibrary(videoModel);
                        }
                    }

                    progressService.IncrementPercentageProgress();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        private void InsertInOrder(PlaylistModel model)
        {
            for (int i = 0; i < _playlists.Count; i++)
            {
                if (string.Compare(_playlists[i].Name, model.Name, StringComparison.Ordinal) > 0)
                {
                    _playlists.Insert(i, model);
                    return;
                }
            }
            _playlists.Add(model);
        }

        private async Task<YouTubeService> GetService()
        {
            try
            {
                UserCredential credential = await _platformService.GetGoogleWebAuthorizationCredentials();

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = GetType().ToString()
                });
                return youtubeService;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        public async Task<bool> AddNewPlaylistByLink(string link)
        {
            try
            {
                var youtubeService = await GetService();

                if (youtubeService != null)
                {
                    var id = link.Substring(link.LastIndexOf("list=", StringComparison.Ordinal) + "list=".Length);

                    var playlistrequestTask = youtubeService.Playlists.List("snippet, contentDetails");
                    playlistrequestTask.Id = id;

                    var response = await playlistrequestTask.ExecuteAsync();
                    if (response.Items.Any())
                    {
                        var model = new PlaylistModel()
                        {
                            Id = response.Items[0].Id,
                            Name = response.Items[0].Snippet.Title,
                        };
                        InsertInOrder(model);

                        await _settingsRepository.SaveCache();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public ObservableCollection<PlaylistModel> GetDesignCollection()
        {
            return new ObservableCollection<PlaylistModel>()
            {
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(false),
                GetExamplePlaylist(false),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
            };
        }

        private PlaylistModel GetExamplePlaylist(bool refresh = true)
        {
            var playlist = new PlaylistModel()
            {
                Name = "Music playlist",
                Id = "ajdkga8rldhs7",
                Refresh = refresh,
                Videos = new ObservableCollection<VideoModel>()
                {
                    GetExampleVideo(SaveStatus.Discovered),
                    GetExampleVideo(SaveStatus.DownloadPending),
                    GetExampleVideo(SaveStatus.Downloading),
                    GetExampleVideo(SaveStatus.Downloaded),
                    GetExampleVideo(SaveStatus.Converting),
                    GetExampleVideo(SaveStatus.Converted),
                    GetExampleVideo(SaveStatus.FillingAutomaticProperties),
                    GetExampleVideo(SaveStatus.FilledAutomaticProperties),
                    GetExampleVideo(SaveStatus.Saving),
                    GetExampleVideo(SaveStatus.Saved),
                    GetExampleVideo(),
                    GetExampleVideo(),
                    GetExampleVideo(),
                    GetExampleVideo(SaveStatus.FailedDownloadOrConversion)
                }
            };
            foreach (var videoModel in playlist.Videos)
            {
                videoModel.PlaylistModel = playlist;
            }
            return playlist;
        }

        private VideoModel GetExampleVideo(SaveStatus status = SaveStatus.Finished)
        {
            var vm = new VideoModel()
            {
                Id = "hja7wdjka7ef8af6asdf6",
                Mp3Model = GetExampleMp3(),
                Name = "Video name - Alle farben (" + status + ")",
                SaveStatus = status
            };
            vm.Mp3Model.VideoModel = vm;
            if (vm.SaveStatus < SaveStatus.Finished)
            {
                vm.ProgressService = new ProgressService();
                vm.ProgressService.ConfigurePercentageProgress(100, 20);
            }
            return vm;
        }

        private Mp3Model GetExampleMp3()
        {
            var mp3 = new Mp3Model()
            {
                Album = "Album alle Farben",
                AlbumArtist = "Artists für Album",
                Artist = "Artists für President",
                Genre = "Genre des Dinos",
                Title = "Grösser Titer",
                Year = 2014,
                FilePath = "8213ää123ö12ü713äa/Artist - Song.mp3"
            };
            return mp3;
        }
    }
}
