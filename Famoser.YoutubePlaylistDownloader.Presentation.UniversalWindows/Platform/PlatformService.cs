using System;
using System.Threading;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.YouTube.v3;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform
{
    class PlatformService : IPlatformService
    {
        public async Task<UserCredential> GetGoogleWebAuthorizationCredentials()
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                                  new Uri("ms-appx:///Assets/Apis/client_id.json"),
                                  // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                                  // user's account, but not other types of account access.
                                  new[] { YouTubeService.Scope.YoutubeReadonly },
                                  "user",
                                  CancellationToken.None);
        }
    }
}
