using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Singleton;
using GalaSoft.MvvmLight.Ioc;
using TagLib;
using YoutubeExtractor;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services.Interfaces;
using File = TagLib.File;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class DownloadService : SingletonBase<DownloadService>
    {
        public async Task<bool> DownloadYoutubeVideo(VideoModel vm, string folder, ProgressModel pm)
        {
            try
            {
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(vm.Link);

                /*
                 * We want the first extractable video with the highest audio quality.
                 */
                VideoInfo video = videoInfos
                    .Where(info => info.CanExtractAudio)
                    .OrderByDescending(info => info.AudioBitrate)
                    .FirstOrDefault();

                if (video == null)
                    return false;

                /*
                 * If the video has a decrypted signature, decipher it
                 */
                if (video.RequiresDecryption)
                {
                    DownloadUrlResolver.DecryptDownloadUrl(video);
                }

                /*
                 * Create the audio downloader.
                 * The first argument is the video where the audio should be extracted from.
                 * The second argument is the path to save the audio file.
                 */
                var audioDownloader = new AudioDownloader(video, Path.Combine(folder, vm.Id + video.AudioExtension));

                // Register the progress events. We treat the download progress as 85% of the progress and the extraction progress only as 15% of the progress,
                // because the download will take much longer than the audio extraction.
                audioDownloader.DownloadProgressChanged += (sender, args) => SimpleIoc.Default.GetInstance<IProgressService>().SetProgress(pm, (int)(args.ProgressPercentage * 0.85));
                audioDownloader.AudioExtractionProgressChanged += (sender, args) => SimpleIoc.Default.GetInstance<IProgressService>().SetProgress(pm, 85 + (int)(args.ProgressPercentage * 0.15));

                /*
                 * Execute the video downloader.
                 * For GUI applications note, that this method runs synchronously.
                 */
                await Task.Run(() => audioDownloader.Execute());

                SimpleIoc.Default.GetInstance<IProgressService>().RemoveProgress(pm);


                return true;

            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<IPicture> GetAlbumArt(Uri url)
        {
            try
            {
                if (url != null)
                {
                    HttpClient client = new HttpClient();
                    var bytes = await client.GetByteArrayAsync(url);
                    var vektor = new ByteVector(bytes);
                    IPicture newArt = new Picture(vektor);
                    return newArt;
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
