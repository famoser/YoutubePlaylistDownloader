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
        
        public static List<VideoModel> Convert(IList<VideoCacheModel> models)
        {
            return models.Select(Convert).ToList();
        }

        public static List<VideoCacheModel> Convert(IList<VideoModel> models)
        {
            return models.Select(Convert).ToList();
        }

        public static PlaylistCacheModel Convert(PlaylistModel model)
        {
            return new PlaylistCacheModel()
            {
                Id = model.Id,
                Download = model.Download,
                DownloadedVideos = Convert(model.DownloadedVideos),
                FailedVideos = Convert(model.FailedVideos)
            };
        }

        public static List<PlaylistCacheModel> Convert(IList<PlaylistModel> models)
        {
            return models.Select(Convert).ToList();
        } 
    }
}
