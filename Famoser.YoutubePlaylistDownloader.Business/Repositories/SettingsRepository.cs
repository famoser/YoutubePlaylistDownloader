using System;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Base;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SettingsRepository : BaseService, ISettingsRepository
    {
        private ConfigurationModel _config;
        private CacheModel _cache;
        private readonly IFolderStorageService _folderStorageService;

        public SettingsRepository(IFolderStorageService folderStorageService) : base(true, LogHelper.Instance)
        {
            _folderStorageService = folderStorageService;
        }

        private bool _isInitialized = false;
        private Task Initialize()
        {
            return Execute(async () =>
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
            });
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

        public Task<bool> SaveCache()
        {
            return Execute(async () =>
            {
                var converter = new PlaylistConverter();
                var cache = new CacheModel()
                {
                    CachedPlaylists = converter.Convert(SimpleIoc.Default.GetInstance<IPlaylistRepository>().GetPlaylists())
                };

                return await _folderStorageService.SetCachedTextFileAsync(FileKeys.CacheFile.ToString(),
                    JsonConvert.SerializeObject(cache));
            });
        }
    }
}
