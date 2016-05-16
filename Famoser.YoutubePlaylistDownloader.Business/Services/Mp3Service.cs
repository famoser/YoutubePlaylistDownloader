using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.Singleton;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using GalaSoft.MvvmLight.Ioc;
using IF.Lastfm.Core.Api;
using TagLib;

namespace Famoser.YoutubePlaylistDownloader.Business.Services
{
    public class Mp3Service
    {

        public static List<Mp3Model> GetSaveReadyModels(IList<Mp3Model> models)
        {
            //some validation
            var artists = new List<string>();
            foreach (var mp3Model in models)
            {
                if (!string.IsNullOrEmpty(mp3Model.Artist) && artists.Contains(mp3Model.Artist))
                    artists.Add(mp3Model.Artist);
            }

            var resModels = models.Where(m => m.AllImportantPropertiesFilled).ToList();

            for (int index = 0; index < resModels.Count; index++)
            {
                var mp3Model = resModels[index];
                if (artists.Contains(mp3Model.Title))
                {
                    resModels.Remove(mp3Model);
                    index--;
                }
            }


            return resModels;
        }
    }
}
