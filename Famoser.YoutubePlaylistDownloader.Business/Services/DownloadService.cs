﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.FrameworkEssentials.Singleton;
using Famoser.YoutubeExtractor.Portable.Downloaders;
using Famoser.YoutubeExtractor.Portable.Helpers;
using Famoser.YoutubeExtractor.Portable.Models;
using GalaSoft.MvvmLight.Ioc;
using TagLib;
using YoutubePlaylistDownloader.Business.Models;
using File = TagLib.File;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class DownloadService : SingletonBase<DownloadService>
    {
        public async Task<Stream> DownloadYoutubeVideo(VideoModel vm, string folder, IProgressService service)
        {
            try
            {
                var downloader = new AudioDownloader();

                //download video infos
                IEnumerable<VideoInfo> videoInfos = await DownloadUrlResolver.GetDownloadUrlsAsync("https://www.youtube.com/watch?v=vxMxYgkUcdU");

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