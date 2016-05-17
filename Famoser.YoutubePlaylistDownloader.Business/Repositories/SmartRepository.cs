using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using IF.Lastfm.Core.Api;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class SmartRepository : ISmartRepository
    {
        private readonly ISettingsRepository _settingsRepository;

        public SmartRepository(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public void AssignMetaTags(PlaylistModel list)
        {
            foreach (var model in list.Videos)
            {
                model.VideoTitle = model.VideoInfo.Name;
                if (model.VideoTitle.Contains("-"))
                {
                    var split = model.VideoTitle.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    model.Title = split[1].Trim();
                    model.Artist = split[0].Trim();
                }
                else
                {
                    model.Title = model.VideoInfo.Name;
                }

                model.Comment = model.VideoInfo.Id;
                model.Year = (uint)DateTime.Now.Year;
                model.AlbumArtist = "famoser";
                model.Album = "yout: " + list.Name;
                model.Genre = list.Name;
            }
        }

        public void AssignMetaTags(IList<PlaylistModel> list)
        {
            foreach (var playlistModel in list)
            {
                AssignMetaTags(playlistModel);
            }
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
