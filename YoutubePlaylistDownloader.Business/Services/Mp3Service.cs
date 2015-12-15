using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Florianalexandermoser.Common.Patterns.Singleton;
using Florianalexandermoser.Common.Utils.Logs;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using TagLib;
using YoutubePlaylistDownloader.Business.Models;
using File = System.IO.File;

namespace YoutubePlaylistDownloader.Business.Services
{
    public class Mp3Service : SingletonBase<Mp3Service>
    {
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
                LogHelper.Instance.LogExeption(ex);
            }
            return null;
        }

        public async Task SaveModel(Mp3Model model, bool finish)
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

            if (finish)
                File.Move(model.FilePath, model.TargetFilePath);
        }
    }
}
