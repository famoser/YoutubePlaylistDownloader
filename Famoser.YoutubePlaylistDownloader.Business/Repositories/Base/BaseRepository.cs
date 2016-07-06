using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Base
{
    public class BaseRepository
    {
        protected T Execute<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex, this);
            }
            return default(T);
        }
    }
}
