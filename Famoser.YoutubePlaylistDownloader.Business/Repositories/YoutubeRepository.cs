using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubeDataApiWrapper.Portable.RequestBuilders;
using Famoser.YoutubeDataApiWrapper.Portable.RequestServices;
using Famoser.YoutubeDataApiWrapper.Portable.Util;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class YoutubeRepository : IYoutubeRepository
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly IPlatformService _platformService;

        public YoutubeRepository(ISettingsRepository settingsRepository, IPlatformService platformService)
        {
            _settingsRepository = settingsRepository;
            _platformService = platformService;
        }

        private ObservableCollection<PlaylistModel> _list = new ObservableCollection<PlaylistModel>();
        public async Task<ObservableCollection<PlaylistModel>> GetPlaylists()
        {
            var cache = await _settingsRepository.GetCache();
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

                var res = new List<PlaylistModel>();
                foreach (var rawPlaylist in rawPlaylists)
                {
                    var model = new PlaylistModel()
                    {
                        Id = rawPlaylist.Id,
                        Name = rawPlaylist.Snippet.Title,
                    };
                    var oldOne = cache.CachedPlaylists.FirstOrDefault(p => p.Id == rawPlaylist.Id);
                    if (oldOne != null)
                    {
                        model.DownloadedVideos = ConverterHelper.Convert(oldOne.DownloadedVideos);
                        model.FailedVideos = ConverterHelper.Convert(oldOne.FailedVideos);
                        model.Download = oldOne.Download;
                    }

                    model.TotalVideos = rawPlaylist.ContentDetails.ItemCount.HasValue
                        ? (int)rawPlaylist.ContentDetails.ItemCount.Value
                        : 0;
                    res.Add(model);
                }
                _list = new ObservableCollection<PlaylistModel>(res.OrderBy(p => p.Name));
            }

            return _list;
        }

        public async Task<bool> DownloadVideos(IProgressService progressService)
        {
            foreach (var playlist in _list.Where(p => p.Download))
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

                        playlist.NewFiles.Add(mp3File);
                    }

                    progressService.IncrementPercentageProgress();
                }
            }
            return true;
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



        public async Task<PlaylistModel> GetPlaylistByLink(string link)
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
                        //if (rawPlaylist.ContentDetails)
                        var model = new PlaylistModel()
                        {
                            Id = response.Items[0].Id,
                            Name = response.Items[0].Snippet.Title,

                        };
                        model.TotalVideos = response.Items[0].ContentDetails.ItemCount.HasValue
                            ? (int)response.Items[0].ContentDetails.ItemCount.Value
                            : 0;

                        return model;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }
    }
}
