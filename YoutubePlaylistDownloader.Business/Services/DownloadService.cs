using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Florianalexandermoser.Common.Patterns.Singleton;
using GalaSoft.MvvmLight.Ioc;
using YoutubeExtractor;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services.Interfaces;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class DownloadService : SingletonBase<DownloadService>
    {
        public async Task<bool> DownloadYoutubeVideo(VideoModel vm, string folder)
        {
            IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(vm.Link);

            /*
             * We want the first extractable video with the highest audio quality.
             */
            VideoInfo video = videoInfos
                .Where(info => info.CanExtractAudio)
                .OrderByDescending(info => info.AudioBitrate)
                .First();

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
            var pm = new ProgressModel()
            {
                Description = "Downloading " + vm.Name
            };
            audioDownloader.DownloadProgressChanged += (sender, args) => SimpleIoc.Default.GetInstance<IProgressService>().SetProgress(pm, (int)(args.ProgressPercentage * 0.85));
            audioDownloader.AudioExtractionProgressChanged += (sender, args) => SimpleIoc.Default.GetInstance<IProgressService>().SetProgress(pm, (int)(args.ProgressPercentage * 0.15));

            /*
             * Execute the video downloader.
             * For GUI applications note, that this method runs synchronously.
             */
            await Task.Run(() => audioDownloader.Execute());

            SimpleIoc.Default.GetInstance<IProgressService>().RemoveProgress(pm);


            return true;
        }
    }
}
