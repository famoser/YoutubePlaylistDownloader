using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;

namespace Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IPlatformService
    {
        Task<UserCredential> GetGoogleWebAuthorizationCredentials();
    }
}
