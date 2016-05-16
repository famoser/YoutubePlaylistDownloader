using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using IF.Lastfm.Core.Api;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SmartRepository : ISmartRepository
    {
        private ISettingsRepository _settingsRepository;

        public SmartRepository(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Uri> GetAlbumCoverUri(Mp3Model model)
        {
            try
            {
                var config = await _settingsRepository.GetConfiguration();
                var client = new LastfmClient(config.FmApiKey, config.FmApiSecred);

                if (model.Title != null && model.Artist != null)
                {
                    var trackResponse = await client.Track.GetInfoAsync(model.Title, model.Artist);
                    if (trackResponse.Success)
                    {
                        if ((model.Album == model.Artist || model.Album == null) && trackResponse.Content.AlbumName != null)
                        {
                            model.Album = trackResponse.Content.AlbumName;
                        }
                        if (trackResponse.Content.Images?.Large != null)
                        {
                            return trackResponse.Content?.Images?.Large;
                        }
                    }
                }


                if (model.Album != model.Artist)
                {
                    var response = await client.Album.GetInfoAsync(model.Artist, model.Album, true);
                    if (response.Success)
                    {
                        return response.Content.Images?.Large;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }
    }
}
