using System;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Base;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Base;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using Nito.AsyncEx;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SettingsRepository : BaseRepository, ISettingsRepository
    {
        private ConfigurationModel _config;
        private CacheModel _cache;
        private readonly IFolderStorageService _folderStorageService;

        public SettingsRepository(IFolderStorageService folderStorageService)
        {
            _folderStorageService = folderStorageService;
        }

        private bool _isInitialized;
        private readonly AsyncLock _initializeAsyncLock = new AsyncLock();
        private Task Initialize()
        {
            return Execute(async () =>
            {
                using (await _initializeAsyncLock.LockAsync())
                {
                    if (_isInitialized)
                        return;

                    _isInitialized = true;

                    var json = await Execute(async () => await _folderStorageService.GetCachedTextFileAsync(FileKeys.CacheFile.ToString()));
                    _cache = json != null ? JsonConvert.DeserializeObject<CacheModel>(json) : new CacheModel();
                    await SaveCache();

                    json = await Execute(async () => await _folderStorageService.GetUserTextFileAsync(FileKeys.ConfigurationFile.ToString()));
                    _config = json != null ? JsonConvert.DeserializeObject<ConfigurationModel>(json) : new ConfigurationModel();
                }
            });
        }

        public async Task<ConfigurationModel> GetConfiguration()
        {
            await Initialize();
            return _config ?? new ConfigurationModel();
        }

        public async Task<CacheModel> GetCache()
        {
            await Initialize();
            return _cache ?? new CacheModel();
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
