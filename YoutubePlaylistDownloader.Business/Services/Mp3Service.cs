using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Singleton;
using GalaSoft.MvvmLight.Ioc;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using TagLib;
using YoutubePlaylistDownloader.Business.Models;
using YoutubePlaylistDownloader.Business.Services.Interfaces;
using File = System.IO.File;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class Mp3Service : SingletonBase<Mp3Service>
    {
        private IProgressService _progressService;

        public Mp3Service()
        {
            _progressService = SimpleIoc.Default.GetInstance<IProgressService>();
        }

        public Mp3Model MakeModel(string filepath)
        {
            using (var fileStream = new FileStream(filepath, FileMode.Open))
            {
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(fileStream.Name,
                    fileStream, fileStream));

                var tags = tagFile.GetTag(TagTypes.Id3v2);
                var model = new Mp3Model()
                {
                    Album = tags.Album,
                    AlbumArtist = tags.AlbumArtists.FirstOrDefault(),
                    Artist = tags.Performers.FirstOrDefault(),
                    Comment = tags.Comment,
                    Genre = tags.Genres.FirstOrDefault(),
                    Title = tags.Title,
                    Year = tags.Year,
                    FilePath = filepath
                };
                return model;
            }
        }

        public async Task<Uri> GetAlbumCoverUri(Mp3Model model)
        {
            try
            {
                var client = new LastfmClient("8bec83c69d9fd33c71b5772b2d9fcd6a", "Od13f6a9ee794acaf88dce4fb9a52c62");

                if (model.Title != null && model.Artist != null)
                {
                    var trackResponse = await client.Track.GetInfoAsync(model.Title, model.Artist);
                    if (trackResponse.Success)
                    {
                        if ((model.Album == model.Artist || model.Album == null) && trackResponse.Content.AlbumName != null)
                        {
                            model.Album = trackResponse.Content.AlbumName;
                        }
                        if (trackResponse.Content.Images?.Large != null)
                        {
                            return trackResponse.Content?.Images?.Large;
                        }
                    }
                }


                if (model.Album != model.Artist)
                {
                    var response = await client.Album.GetInfoAsync(model.Artist, model.Album, true);
                    if (response.Success)
                    {
                        return response.Content.Images?.Large;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        public async Task<List<Mp3Model>> GetSaveReadyModels(IList<Mp3Model> models)
        {
            return await Task.Run(() =>
            {
                //some validation
                var artists = new List<string>();
                foreach (var mp3Model in models)
                {
                    if (!string.IsNullOrEmpty(mp3Model.Artist) && artists.Contains(mp3Model.Artist))
                        artists.Add(mp3Model.Artist);
                }

                var resModels = models.Where(m => m.AllImportantPropertiesFilled).ToList();

                for (int index = 0; index < resModels.Count; index++)
                {
                    var mp3Model = resModels[index];
                    if (artists.Contains(mp3Model.Title))
                    {
                        resModels.Remove(mp3Model);
                        index--;
                    }
                }


                return resModels;
            });
        }

        public async Task SaveAllModels(IList<Mp3Model> model, bool moveTotargetFolder)
        {
            for (int index = 0; index < model.Count; index++)
            {
                var mp3Model = model[index];
                var pm = new ProgressModel()
                {
                    Description = "Saving " + (index + 1) + " / " + model.Count + ")"
                };
                _progressService.SetProgress(pm, index);
                await SaveModel(mp3Model, moveTotargetFolder);
                _progressService.RemoveProgress(pm);
            }
        }

        public async Task SaveModel(Mp3Model model, bool moveTotargetFolder)
        {
            try
            {
                using (var fileStream = new FileStream(model.FilePath, FileMode.Open))
                {
                    var tagFile = TagLib.File.Create(new StreamFileAbstraction(fileStream.Name,
                        fileStream, fileStream));

                    //to avoid null reference exception
                    if (model.Genre == null)
                        model.Genre = "";
                    if (model.AlbumArtist == null)
                        model.AlbumArtist = "";
                    if (model.Artist == null)
                        model.Artist = "";

                    tagFile.Tag.Album = model.Album;
                    tagFile.Tag.AlbumArtists = new[] { model.AlbumArtist };
                    tagFile.Tag.Performers = new[] { model.Artist };
                    tagFile.Tag.Title = model.Title;
                    tagFile.Tag.Comment = model.Comment;
                    tagFile.Tag.Genres = new[] { model.Genre };
                    tagFile.Tag.Year = model.Year;
                    if (model.AlbumCover != null)
                    {
                        var picture = await DownloadService.Instance.GetAlbumArt(model.AlbumCover);
                        if (picture != null)
                        {
                            tagFile.Tag.Pictures = new IPicture[1] { picture };
                        }
                    }
                    tagFile.Save();
                }

                if (moveTotargetFolder)
                    File.Move(model.FilePath, model.TargetFilePath);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
        }
    }
}
