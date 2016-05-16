using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers
{
    public class ConverterHelper
    {
        public static VideoCacheModel Convert(VideoModel model)
        {
            return new VideoCacheModel()
            {
                Id = model.Id,
                FileName = model.FileName
            };
        }

        public static VideoModel Convert(VideoCacheModel model)
        {
            return new VideoModel()
            {
                Id = model.Id,
                FileName = model.FileName
            };
        }
        
        public static List<VideoModel> Convert(List<VideoCacheModel> models)
        {
            return models.Select(Convert).ToList();
        }

        public static List<VideoCacheModel> Convert(List<VideoModel> models)
        {
            return models.Select(Convert).ToList();
        }
    }
}
