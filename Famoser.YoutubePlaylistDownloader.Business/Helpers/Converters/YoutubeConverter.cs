using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters
{
    public abstract class YoutubeConverter<TCache, TModel> : BaseConverter<TCache, TModel> where TCache : YoutubeCacheModel, new() where TModel : YoutubeModel, new()
    {
        public override TCache Convert(TModel model)
        {
            if (model != null)
            {
                return new TCache()
                {
                    Id = model.Id,
                    Name = model.Name
                };
            }
            return null;
        }

        public override TModel Convert(TCache model)
        {
            if (model != null)
            {
                return new TModel()
                {
                    Id = model.Id,
                    Name = model.Name
                };
            }
            return null;
        }
    }
}
