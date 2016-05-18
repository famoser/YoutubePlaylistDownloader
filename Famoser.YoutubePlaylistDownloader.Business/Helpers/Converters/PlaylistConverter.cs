using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters
{
    public class PlaylistConverter : YoutubeConverter<PlaylistCacheModel, PlaylistModel>
    {
        public override PlaylistCacheModel Convert(PlaylistModel model)
        {
            if (model != null)
            {
                var cacheModel = base.Convert(model);
                cacheModel.Refresh = model.Refresh;
                var converter = new VideoConverter();
                cacheModel.VideoCacheModels = converter.Convert(model.Videos);
                return cacheModel;
            }
            return null;
        }

        public override PlaylistModel Convert(PlaylistCacheModel model)
        {
            if (model != null)
            {
                var cacheModel = base.Convert(model);
                cacheModel.Refresh = model.Refresh;
                var converter = new VideoConverter();
                cacheModel.Videos = converter.Convert(model.VideoCacheModels);
                return cacheModel;
            }
            return null;
        }
    }
}
