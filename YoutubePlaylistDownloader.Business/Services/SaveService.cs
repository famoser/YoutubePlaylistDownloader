using System;
using System.IO;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Singleton;
using GalaSoft.MvvmLight.Ioc;
using Newtonsoft.Json;
using YoutubePlaylistDownloader.Business.Models;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class SaveService : SingletonBase<SaveService>
    {
        private const string Path = "config.json";
        public void SaveState(SaveModel saveModel)
        {
            try
            {
                var str = JsonConvert.SerializeObject(saveModel);
                File.WriteAllText(Path, str);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
        }

        public SaveModel RetrieveState()
        {
            try
            {
                if (File.Exists(Path))
                {
                    var str = File.ReadAllText(Path);
                    return JsonConvert.DeserializeObject<SaveModel>(str);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return new SaveModel();
        }
    }
}
