using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeDataApiWrapper.RequestBuilders;
using YouTubeDataApiWrapper.RequestServices;
using YouTubeDataApiWrapper.Util;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class YoutubeService
    {
        public async Task<IEnumerable<object>> DownloadPlaylists()
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

                var channelrequestTask = youtubeService.Channels.List("id");
                channelrequestTask.Mine = true;

                var channelrequest = await channelrequestTask.ExecuteAsync();
                var channelId = channelrequest.Items[0].Id;

                var playlistrequestTask = youtubeService.Playlists.List("snippet");
                playlistrequestTask.ChannelId = channelId;
                var lists = await playlistrequestTask.ExecuteAsync();

                //Get 5000 videos from a uploads playlist


                //Get 5000 videos from a uploads playlist
                var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService, "snippet")
                {
                    PlaylistId = "UUsvaJro-UrvEQS9_TYsdAzQ"
                };
                var playlistItemsRequestService =
                    new YoutubeListRequestService<PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                        (plistItemsListRequestBuilder);

                var obj = await playlistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000));


                //if (rId.getKind().equals("youtube#video"))
                //{

                //}

                return obj;

            }
            catch (Exception ex)
            {
                
                ex.ToString();
            }
            return new List<object>();


        }


        public async Task<IEnumerable<object>> DownloadVideos()
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

                var channelrequestTask = youtubeService.Channels.List("id");
                channelrequestTask.Mine = true;

                var channelrequest = await channelrequestTask.ExecuteAsync();
                var channelId = channelrequest.Items[0].Id;

                var playlistrequestTask = youtubeService.Playlists.List("snippet");
                playlistrequestTask.ChannelId = channelId;
                var lists = await playlistrequestTask.ExecuteAsync();

                //Get 5000 videos from a uploads playlist


                //Get 5000 videos from a uploads playlist
                var plistItemsListRequestBuilder = new PlaylistItemsListRequestBuilder(youtubeService, "snippet")
                {
                    PlaylistId = "UUsvaJro-UrvEQS9_TYsdAzQ"
                };
                var playlistItemsRequestService =
                    new YoutubeListRequestService<PlaylistItemsResource.ListRequest, PlaylistItemListResponse, PlaylistItem>
                        (plistItemsListRequestBuilder);

                var obj = await playlistItemsRequestService.ExecuteConcurrentAsync(new PageTokenRequestRange(5000));


                //if (rId.getKind().equals("youtube#video"))
                //{

                //}

                return obj;

            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            return new List<object>();
        }
    }
}
