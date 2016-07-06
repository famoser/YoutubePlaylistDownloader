using System;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Base;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Base;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using IF.Lastfm.Core.Api;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SmartRepository : BaseRepository, ISmartRepository
    {
        private readonly ISettingsRepository _settingsRepository;

        public SmartRepository(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        private void AssignMetaTags(Mp3Model model)
        {
            model.Title = model.Title.Trim();
            if (model.Title.Contains("-"))
            {
                var split = model.Title.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                model.Title = split[1].Trim();
                model.Artist = split[0].Trim();
            }

            model.Year = (uint)DateTime.Now.Year;
            model.AlbumArtist = "famoser";
            model.Album = "yout: " + model.VideoModel.PlaylistModel.Name;
            model.Genre = model.VideoModel.PlaylistModel.Name;
        }

        private Task<Uri> GetAlbumCoverUri(Mp3Model model)
        {
            return Execute(async () =>
            {
                var config = await _settingsRepository.GetConfiguration();
                var client = new LastfmClient(config.FmApiKey, config.FmApiSecred);

                if (model.Title != null && model.Artist != null)
                {
                    var trackResponse = await client.Track.GetInfoAsync(model.Title, model.Artist);
                    if (trackResponse.Success)
                    {
                        if ((model.Album == model.Artist || model.Album == null) &&
                            trackResponse.Content.AlbumName != null)
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
                return null;
            });
        }

        public Task<bool> FillAutomaticProperties(Mp3Model model)
        {
            return Execute(async () =>
            {
                model.VideoModel.SaveStatus = SaveStatus.FillingAutomaticProperties;
                AssignMetaTags(model);
                var uri = await GetAlbumCoverUri(model);
                if (uri != null)
                {
                    model.AlbumCover = await DownloadHelper.DownloadBytes(uri);
                }

                model.VideoModel.SaveStatus = SaveStatus.FilledAutomaticProperties;
                return true;
            });
        }
    }
}
