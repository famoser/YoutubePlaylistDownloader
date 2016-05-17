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
        private readonly IMp3Respository _mp3Respository;
        private readonly ISmartRepository _smartRepository;
        private readonly IPlatformService _platformService;

        public PlaylistRepository(ISettingsRepository settingsRepository, IPlatformService platformService, IMp3Respository mp3Respository, ISmartRepository smartRepository)
        {
            _settingsRepository = settingsRepository;
            _platformService = platformService;
            _mp3Respository = mp3Respository;
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

        public async Task<bool> RefreshPlaylists(IProgressService progressService)
        {
            try
            {
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
                        if (_playlists.All(l => l.Id != rawPlaylist.Id))
                        {
                            var model = new PlaylistModel()
                            {
                                Id = rawPlaylist.Id,
                                Name = rawPlaylist.Snippet.Title,
                            };
                            InsertInOrder(model);
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<bool> DownloadVideos(IProgressService progressService)
        {
            try
            {
                var tot = _playlists.Sum(l => l.Videos.Count(w => w.SaveStatus < SaveStatus.Finished));
                progressService.ConfigurePercentageProgress(tot);
                foreach (var playlist in _playlists.Where(p => p.Refresh))
                {
                    var vids = await GetVideos(playlist);
                    foreach (var videoModel in vids)
                    {
                        if (videoModel.SaveStatus < SaveStatus.Finished)
                        {
                            videoModel.ProgressServie = new ProgressService();
                            var stream = await DownloadHelper.DownloadYoutubeVideo(videoModel, videoModel.ProgressServie);
                            if (stream != null &&
                                await _mp3Respository.CreateFile(videoModel, stream) &&
                                await _smartRepository.FillAutomaticProperties(videoModel.Mp3Model))
                            {
                                videoModel.SaveStatus = SaveStatus.Finished;

                                await _mp3Respository.SaveFile(videoModel.Mp3Model);
                            }
                        }

                        progressService.IncrementPercentageProgress();
                    }
                }

                //todo: save cache
                progressService.HidePercentageProgress();
                return true;
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

        private async Task<List<VideoModel>> GetVideos(PlaylistModel playlist)
        {
            try
            {
                var youtubeService = await GetService();

                if (youtubeService != null)
                {
                    //Get 5000 videos from a uploads playlist
                    var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService, "snippet, contentDetails")
                    {
                        PlaylistId = playlist.Id
                    };
                    var playlistItemsRequestService =
                        new YoutubeListRequestService
                            <PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                            (plistItemsListRequestBuilder);

                    var obj = await playlistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000));

                    var res = new List<VideoModel>();
                    foreach (var playlistItem in obj)
                    {
                        var model = new VideoModel()
                        {
                            Id = playlistItem.ContentDetails.VideoId,
                            Name = playlistItem.Snippet.Title
                        };
                        res.Add(model);
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return new List<VideoModel>();
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
    }
}
