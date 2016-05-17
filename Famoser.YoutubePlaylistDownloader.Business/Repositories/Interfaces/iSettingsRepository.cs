using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface ISettingsRepository
    {
        Task<ConfigurationModel> GetConfiguration();
        Task<CacheModel> GetCache();
        Task<bool> SaveCache(CacheModel cache);
    }
}
