using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Singleton;
using GalaSoft.MvvmLight.Ioc;
using TagLib;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services.Interfaces;
using File = TagLib.File;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class WorkflowService : SingletonBase<WorkflowService>
    {
        private IProgressService _progressService;

        public WorkflowService()
        {
            _progressService = SimpleIoc.Default.GetInstance<IProgressService>();
        }

        public async Task<List<Mp3Model>> Execute(PlaylistModel playlist, string tempFolder, string targetFolder)
        {
            try
            {
                var pm = new ProgressModel()
                {
                    Description = "Preparing Playlist " + playlist.Name
                };
                _progressService.SetProgress(pm, 0);
                var vids = await YoutubeService.Instance.GetVideos(playlist.Id);
                var tempdic = Path.Combine(tempFolder, playlist.Name);
                var targetdic = Path.Combine(targetFolder, playlist.Name);

                SortOutAlreadyKnownVideos(targetdic, ref vids);
                _progressService.SetProgress(pm, 50);

                PrepareTempFolder(tempdic, ref vids);
                _progressService.SetProgress(pm, 100);

                await DownloadTempFiles(tempdic, vids);

                _progressService.RemoveProgress(pm);
                return RetrieveMp3Models(tempdic, targetdic, vids, playlist);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return new List<Mp3Model>();
        }

        private void SortOutAlreadyKnownVideos(string dic, ref List<VideoModel> vids)
        {
            /* remove already processed files */
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
            else
            {
                string[] files = Directory.GetFiles(dic, "*.mp3", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        using (var fileStream = new FileStream(Path.Combine(dic, file), FileMode.Open))
                        {
                            var tagFile = File.Create(new StreamFileAbstraction(fileStream.Name,
                                             fileStream, fileStream));

                            var tags = tagFile.GetTag(TagTypes.Id3v2);
                            var comm = tags.Comment;
                            var item = vids.FirstOrDefault(v => v.Id == comm);
                            if (item != null)
                                vids.Remove(item);
                        }
                    }
                }
            }
        }

        private void PrepareTempFolder(string dic, ref List<VideoModel> downloadVids)
        {
            /* remove already downloaded temp files */
            if (!Directory.Exists(dic))
                Directory.CreateDirectory(dic);
            else
            {
                string[] files = Directory.GetFiles(dic, "*.mp3", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        var videoId = file.Split(new[] { "." }, StringSplitOptions.None)[0];
                        videoId = videoId.Substring(videoId.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                        var item = downloadVids.FirstOrDefault(v => v.Id == videoId);
                        if (item != null)
                            item.IsDownloaded = true;
                    }
                }
            }
        }

        private async Task DownloadTempFiles(string dic, List<VideoModel> downloadVids)
        {
            var toProcess = downloadVids.Where(v => !v.IsDownloaded).ToList();
            for (int index = 0; index < toProcess.Count; index++)
            {
                var videoModel = toProcess[index];
                var pm = new ProgressModel()
                {
                    Description = "Downloading " + videoModel.Name + " (" + (index + 1) + " / " + toProcess.Count + ")"
                };
                _progressService.SetProgress(pm, 0);

                videoModel.IsDownloaded = await DownloadService.Instance.DownloadYoutubeVideo(videoModel, dic, pm);

                _progressService.RemoveProgress(pm);
            }
        }

        private List<Mp3Model> RetrieveMp3Models(string dic, string targetdic, List<VideoModel> downloadVids, PlaylistModel playlist)
        {
            var res = new List<Mp3Model>();
            string[] files = Directory.GetFiles(dic, "*.mp3", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                foreach (var file in files)
                {
                    var model = Mp3Service.Instance.MakeModel(Path.Combine(dic, file));

                    var videoId = file.Split(new[] { "." }, StringSplitOptions.None)[0];
                    videoId = videoId.Substring(videoId.LastIndexOf("\\", StringComparison.Ordinal) + 1);
                    var item = downloadVids.FirstOrDefault(v => v.Id == videoId);
                    if (item != null)
                    {
                        model.OriginalTitle = item.Name;
                        if (model.OriginalTitle.Contains("-"))
                        {
                            var split = model.OriginalTitle.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                            model.Title = split[1].Trim();
                            model.Artist = split[0].Trim();
                            model.AlbumArtist = "famoser";
                            model.Album = "yout: " + playlist.Name;
                            model.Genre = playlist.Name;
                        }
                        else
                        {
                            model.Title = item.Name;
                        }
                    }
                    else
                    {
                        model.OriginalTitle = videoId;
                    }

                    model.Comment = videoId;
                    model.Year = (uint)DateTime.Now.Year;
                    model.TargetFolder = targetdic;

                    res.Add(model);
                }
            }
            return res;
        }
    }
}
