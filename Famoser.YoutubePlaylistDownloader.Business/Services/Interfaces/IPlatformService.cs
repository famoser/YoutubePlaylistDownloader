using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IPlatformService
    {
        Task<UserCredential> GetGoogleWebAuthorizationCredentials();
    }
}
