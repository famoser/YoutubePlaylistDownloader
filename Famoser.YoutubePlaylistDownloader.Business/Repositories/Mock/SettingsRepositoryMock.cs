using System.Collections.Generic;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Mock
{
    public class SettingsRepositoryMock : ISettingsRepository
    {
        public async Task<ConfigurationModel> GetConfiguration()
        {
            return new ConfigurationModel()
            {

            };
        }

        public async Task<CacheModel> GetCache()
        {
            return new CacheModel()
            {
                CachedPlaylists = new List<PlaylistCacheModel>()
                {
                    new PlaylistCacheModel()
                    {
                        Name = "Playlist 1",
                        Refresh = true
                    },
                    new PlaylistCacheModel()
                    {
                        Name = "Playlist 2",
                        Refresh = false
                    },
                    new PlaylistCacheModel()
                    {
                        Name = "Playlist 3",
                        Refresh = true
                    }
                }
            };
        }

        public async Task<bool> SaveCache()
        {
            return true;
        }
    }
}
