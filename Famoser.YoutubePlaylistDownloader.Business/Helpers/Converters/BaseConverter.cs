using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters
{
    public abstract class BaseConverter<TCache,TModel>
    {
        public abstract TCache Convert(TModel model);

        public abstract TModel Convert(TCache model);
        
        public ObservableCollection<TModel> Convert(IEnumerable<TCache> models)
        {
            return new ObservableCollection<TModel>(models.Select(Convert).ToList());
        }

        public List<TCache> Convert(IEnumerable<TModel> models)
        {
            return models.Select(Convert).ToList();
        }

    }
}
