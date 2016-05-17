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
        private readonly IPlatformService _platformService;

        public PlaylistRepository(ISettingsRepository settingsRepository, IPlatformService platformService)
        {
            _settingsRepository = settingsRepository;
            _platformService = platformService;
        }

        private ObservableCollection<PlaylistModel> _list;
        public async Task<ObservableCollection<PlaylistModel>> GetPlaylists()
        {
            try
            {
                if (_list == null)
                {
                    _list = new ObservableCollection<PlaylistModel>();
                    var cache = await _settingsRepository.GetCache();
                    if (cache != null)
                    {
                        var converter = new PlaylistConverter();
                        foreach (var playlist in cache.CachedPlaylists)
                        {
                            _list.Add(converter.Convert(playlist));
                        }
                    }
                }
                return _list;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
                _list = new ObservableCollection<PlaylistModel>();
            }
            return _list;
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
                        PlaylistModel model;
                        if (_list.All(l => l.Id != rawPlaylist.Id))
                        {
                            model = new PlaylistModel()
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
                foreach (var playlist in _list.Where(p => p.Refresh))
                {
                    var vids = await GetVideos(playlist);
                    progressService.ConfigurePercentageProgress(vids.Count);
                    foreach (var videoModel in vids)
                    {
                        if (playlist.DownloadedVideos.All(v => v.Id != videoModel.Id) &&
                            playlist.FailedVideos.All(v => v.Id != videoModel.Id))
                        {
                            var mp3File = new Mp3Model
                            {
                                DownloadStream =
                                    await DownloadHelper.DownloadYoutubeVideo(videoModel, new ProgressService())
                            };

                            playlist.Videos.Add(mp3File);
                        }

                        progressService.IncrementPercentageProgress();
                    }
                }
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
            for (int i = 0; i < _list.Count; i++)
            {
                if (string.Compare(_list[i].Name, model.Name, StringComparison.Ordinal) > 0)
                {
                    _list.Insert(i, model);
                    return;
                }
            }
            _list.Add(model);
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
