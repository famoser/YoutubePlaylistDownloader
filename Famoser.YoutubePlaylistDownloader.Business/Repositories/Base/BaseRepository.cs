using System;
using Famoser.FrameworkEssentials.Logging;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Base
{
    public class BaseRepository
    {
        protected const int MaxThreads = 5;
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
