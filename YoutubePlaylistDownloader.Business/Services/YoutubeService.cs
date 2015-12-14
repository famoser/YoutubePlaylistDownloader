using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Florianalexandermoser.Common.Patterns.Singleton;
using Florianalexandermoser.Common.Utils.Logs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubePlaylistDownloader.Business.Models;
using YouTubeDataApiWrapper.RequestBuilders;
using YouTubeDataApiWrapper.RequestServices;
using YouTubeDataApiWrapper.Util;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class YoutubeService : SingletonBase<YoutubeService>
    {
        public async Task<List<PlaylistModel>> GetPlaylists()
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

                    var res = new List<PlaylistModel>();
                    foreach (var rawPlaylist in rawPlaylists)
                    {
                        //if (rawPlaylist.ContentDetails)
                        var model = new PlaylistModel()
                        {
                            Id = rawPlaylist.Id,
                            Name = rawPlaylist.Snippet.Title,

                        };
                        model.TotalVideos = rawPlaylist.ContentDetails.ItemCount.HasValue
                            ? (int)rawPlaylist.ContentDetails.ItemCount.Value
                            : 0;
                        res.Add(model);
                    }
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogExeption(ex);
            }
            return new List<PlaylistModel>();
        }


        public async Task<List<VideoModel>> GetVideos(string playlistId)
        {
            try
            {
                var youtubeService = await GetService();

                if (youtubeService != null)
                {
                    //Get 5000 videos from a uploads playlist
                    var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService, "snippet, contentDetails")
                    {
                        PlaylistId = playlistId
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
                LogHelper.Instance.LogExeption(ex);
            }
            return new List<VideoModel>();
        }

        private async Task<YouTubeService> GetService()
        {
            try
            {
                UserCredential credential;
                using (var stream = new FileStream("Files/client_id.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                        // user's account, but not other types of account access.
                        new[] { YouTubeService.Scope.YoutubeReadonly },
                        "user",
                        CancellationToken.None,
                        new FileDataStore(this.GetType().ToString())
                    );
                }

                var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = this.GetType().ToString()
                });
                return youtubeService;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogExeption(ex);
            }
            return null;
        }
    }
}
