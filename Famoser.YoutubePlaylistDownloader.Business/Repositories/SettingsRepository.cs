using System;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Newtonsoft.Json;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private ConfigurationModel _config;
        private CacheModel _cache;
        private readonly IFolderStorageService _folderStorageService;
        private readonly IPlaylistRepository _playlistRepository;

        public SettingsRepository(IFolderStorageService folderStorageService, IPlaylistRepository playlistRepository)
        {
            _folderStorageService = folderStorageService;
            _playlistRepository = playlistRepository;
        }

        private bool _isInitialized = false;
        private async Task Initialize()
        {
            try
            {
                if (!_isInitialized)
                {
                    var json = await _folderStorageService.GetCachedTextFileAsync(FileKeys.CacheFile.ToString());
                    _cache = json != null ? JsonConvert.DeserializeObject<CacheModel>(json) : new CacheModel();

                    json = await _folderStorageService.GetUserTextFileAsync(FileKeys.ConfigurationFile.ToString());
                    _config = json != null
                        ? JsonConvert.DeserializeObject<ConfigurationModel>(json)
                        : new ConfigurationModel();
                }
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
        }

        public async Task<ConfigurationModel> GetConfiguration()
        {
            await Initialize();
            return _config;
        }

        public async Task<CacheModel> GetCache()
        {
            await Initialize();
            return _cache;
        }

        public async Task<bool> SaveCache()
        {
            var converter = new PlaylistConverter();
            var cache = new CacheModel()
            {
                CachedPlaylists = converter.Convert(await _playlistRepository.GetPlaylists())
            };

            return await _folderStorageService.SetCachedTextFileAsync(FileKeys.CacheFile.ToString(),
                JsonConvert.SerializeObject(cache));
        }
    }
}
