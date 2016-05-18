using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubeExtractor.Portable.Downloaders;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers
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
                if (video != null)
                {
                    if (video.RequiresDecryption)
                        await DownloadUrlResolver.DecryptDownloadUrl(video);

                    vm.SaveStatus = SaveStatus.Downloading;
                    // Register the any events
                    downloader.VideoDownloadProgressChanged +=
                        (sender, args) => service.ConfigurePercentageProgress(100, args.ProgressPercentage);
                    downloader.VideoDownloadStarted += (sender, args) => SetNewSaveState(vm, SaveStatus.Downloading);
                    downloader.VideoDownloadFinished += (sender, args) => SetNewSaveState(vm, SaveStatus.Downloaded);

                    downloader.AudioExtractionStarted += (sender, args) => SetNewSaveState(vm, SaveStatus.Converting);
                    downloader.AudioExtractionFinished += (sender, args) => SetNewSaveState(vm, SaveStatus.Converted);

                    var str = await downloader.Execute(video);
                    if (vm.SaveStatus == SaveStatus.Converted)
                        return str;

                    //correct error codes
                    if (vm.SaveStatus == SaveStatus.Downloading)
                        vm.SaveStatus = SaveStatus.FailedDownloading;
                    else if (vm.SaveStatus == SaveStatus.Converting)
                        vm.SaveStatus = SaveStatus.FailedConverting;
                    else
                        vm.SaveStatus = SaveStatus.FailedDownloadOrConversion;
                }
                else
                    vm.SaveStatus = SaveStatus.FailedDownloadingResolvingUrl;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        private static void SetNewSaveState(VideoModel sender, SaveStatus status)
        {
            sender.SaveStatus = status;
        }

        public static async Task<byte[]> DownloadBytes(Uri url)
        {
            try
            {
                if (url != null)
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(url);
                        return bytes;
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
