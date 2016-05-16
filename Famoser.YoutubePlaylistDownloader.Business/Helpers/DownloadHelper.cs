using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.Singleton;
using Famoser.YoutubeExtractor.Portable.Downloaders;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using TagLib;

namespace Famoser.YoutubePlaylistDownloader.Business.Services
{
    public class DownloadHelper
    {
        public static async Task<Stream> DownloadYoutubeVideo(VideoModel vm, IProgressService service)
        {
            try
            {
                var downloader = new AudioDownloader();

                //download video infos
                IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync(vm.Link);

                //Select best suited video (highest resolution)
                VideoInfo video = downloader.ChooseBest(videoInfos);
                if (video.RequiresDecryption)
                    await DownloadUrlResolver.DecryptDownloadUrl(video);

                // Register the any events
                downloader.VideoDownloadProgressChanged += (sender, args) => service.ConfigurePercentageProgress(100, args.ProgressPercentage);

                //Execute the video downloader.
                return await downloader.Execute(video);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        public static async Task<IPicture> GetAlbumArt(Uri url)
        {
            try
            {
                if (url != null)
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(url);
                        var vektor = new ByteVector(bytes);
                        IPicture newArt = new Picture(vektor);
                        return newArt;
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Instance.LogException(e);
            }
            return null;
        }
    }
}
