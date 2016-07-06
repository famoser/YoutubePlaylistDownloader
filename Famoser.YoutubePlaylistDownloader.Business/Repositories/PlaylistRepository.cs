using System;
using System.Collections.Concurrent;
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
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Base;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Nito.AsyncEx;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class PlaylistRepository : BaseRepository, IPlaylistRepository
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
        public ObservableCollection<PlaylistModel> GetPlaylists()
        {
            _playlists = new ObservableCollection<PlaylistModel>();
            Initialize();
            return _playlists;
        }

        private bool _isInitialized;
        private readonly AsyncLock _initializeAsyncLock = new AsyncLock();
        private Task Initialize()
        {
            return Execute(async () =>
            {
                using (await _initializeAsyncLock.LockAsync())
                {
                    if (_isInitialized)
                        return;

                    _isInitialized = true;

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
            });
        }

        private void BacklinkList()
        {
            foreach (var playlistModel in _playlists)
            {
                foreach (var videoModel in playlistModel.Videos)
                {
                    if (videoModel.Mp3Model != null)
                        videoModel.Mp3Model.VideoModel = videoModel;
                    videoModel.PlaylistModel = playlistModel;
                }
            }
        }

        private Task<bool> RefreshPlaylistWorker(ConcurrentStack<PlaylistModel> playlists)
        {
            return Execute(async () =>
            {
                PlaylistModel model;
                while (playlists.TryPop(out model))
                {
                    model.ProgressService.StartIndeterminateProgress(IndeterminateProgressKeys.RefreshPlaylist);
                    var youtubeService = await GetService();

                    if (youtubeService != null)
                    {
                        var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService,
                            "snippet, contentDetails")
                        {
                            PlaylistId = model.Id
                        };
                        var playlistItemsRequestService =
                            new YoutubeListRequestService
                                <PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                                (plistItemsListRequestBuilder);

                        var obj =
                            (await playlistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000)))
                                .ToList();

                        for (int index = 0; index < obj.Count; index++)
                        {
                            var playlistItem = obj.ToList()[index];
                            var video = model.Videos.FirstOrDefault(v => v.Id == playlistItem.ContentDetails.VideoId);

                            if (video != null)
                            {
                                model.Videos.Remove(video);
                                video.Name = playlistItem.Snippet.Title;
                            }
                            else
                            {
                                video = new VideoModel
                                {
                                    Id = playlistItem.ContentDetails.VideoId,
                                    Name = playlistItem.Snippet.Title,
                                    PlaylistModel = model
                                };
                            }
                            if (model.Videos.Count < index)
                                model.Videos.Insert(index, video);
                            else
                                model.Videos.Add(video);
                        }

                        model.ProgressService.StopIndeterminateProgress(IndeterminateProgressKeys.RefreshPlaylist);
                    }
                }
                return true;
            });
        }


        public async Task<bool> RefreshPlaylist(PlaylistModel playlist)
        {
            await Initialize();
            return await Execute(async () =>
            {
                var res = await RefreshPlaylistWorker(new ConcurrentStack<PlaylistModel>(new List<PlaylistModel>() {playlist}));  
                await _settingsRepository.SaveCache();

                return res;
            });
        }

        public async Task<bool> DownloadVideosForPlaylist(PlaylistModel playlist)
        {
            await Initialize();
            return await Execute(async () =>
            {
                var res = await DownloadVideosForPlaylistWorker(new ConcurrentStack<PlaylistModel>(new List<PlaylistModel>() { playlist }));
                await _settingsRepository.SaveCache();
                return res;
            });
        }

        public async Task<bool> RefreshAllPlaylists()
        {
            await Initialize();
            return await Execute(async () =>
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

                    var ids = new List<string>();
                    foreach (var rawPlaylist in rawPlaylists)
                    {
                        if (_playlists.All(l => l.Id != rawPlaylist.Id))
                        {
                            var playlist = new PlaylistModel()
                            {
                                Id = rawPlaylist.Id,
                                Name = rawPlaylist.Snippet.Title,
                            };
                            InsertInOrder(playlist);
                        }
                        ids.Add(rawPlaylist.Id);
                    }

                    var oldOnes = _playlists.Where(p => ids.All(i => i != p.Id)).ToList();
                    foreach (var playlistModel in oldOnes)
                    {
                        _playlists.Remove(playlistModel);
                    }
                    
                    var playlists = new ConcurrentStack<PlaylistModel>();
                    foreach (var source in _playlists.Where(p => p.Refresh))
                    {
                        playlists.Push(source);
                    }
                    var tasks = new List<Task>();
                    for (int i = 0; i < MaxThreads; i++)
                    {
                        tasks.Add(RefreshPlaylistWorker(playlists));
                    }
                    await Task.WhenAll(tasks);
                    await _settingsRepository.SaveCache();
                    
                    return true;
                }
                return false;
            });
        }

        public async Task<bool> DownloadVideosForAllPlaylists()
        {
            await Initialize();
            return await Execute(async () =>
            {
                var playlists = new ConcurrentStack<PlaylistModel>(_playlists.Where(p => p.Refresh));
                var tasks = new List<Task>();
                for (int i = 0; i < MaxThreads; i++)
                {
                    tasks.Add(DownloadVideosForPlaylistWorker(playlists));
                }
                await Task.WhenAll(tasks);
                await _settingsRepository.SaveCache();
                return true;
            });
        }

        private async Task<bool> DownloadVideosForPlaylistWorker(ConcurrentStack<PlaylistModel> playlists)
        {
            await Initialize();
            return await Execute(async () =>
            {
                PlaylistModel model;
                while (playlists.TryPop(out model))
                {
                    model.ProgressService.ConfigurePercentageProgress(model.Videos.Count);
                    foreach (var videoModel in model.Videos)
                    {
                        if (videoModel.SaveStatus < SaveStatus.Finished)
                        {
                            var stream = await DownloadHelper.DownloadYoutubeVideo(videoModel, videoModel.ProgressService);
                            if (stream != null &&
                                await _videoRespository.CreateToMusicLibrary(videoModel, stream) &&
                                await _smartRepository.FillAutomaticProperties(videoModel.Mp3Model))
                            {
                                videoModel.SaveStatus = SaveStatus.Finished;

                                await _videoRespository.SaveToMusicLibrary(videoModel);
                            }
                        }

                        model.ProgressService.IncrementPercentageProgress();
                    }
                    model.ProgressService.HidePercentageProgress();
                }
                return true;
            });
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
            await Initialize();
            return await Execute(async () =>
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
                return false;
            });
        }
    }
}
