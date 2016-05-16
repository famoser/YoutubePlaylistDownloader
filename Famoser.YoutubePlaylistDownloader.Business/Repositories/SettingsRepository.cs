using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Newtonsoft.Json;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private ConfigurationModel _config;
        private CacheModel _cache;
        private IStorageService _storageService;

        public SettingsRepository(IStorageService storageService)
        {
            _storageService = storageService;
        }

        private bool _isInitialized = false;
        private async Task Initialize()
        {
            try
            {
                if (!_isInitialized)
                {
                    var json = await _storageService.GetCachedTextFileAsync(FileKeys.CacheFile.ToString());
                    _cache = json != null ? JsonConvert.DeserializeObject<CacheModel>(json) : new CacheModel();

                    json = await _storageService.GetUserTextFileAsync(FileKeys.ConfigurationFile.ToString());
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
    }
}
