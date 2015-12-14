using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Florianalexandermoser.Common.Patterns.Singleton;
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

        public void SaveModel(Mp3Model model)
        {
            using (var fileStream = new FileStream(model.FilePath, FileMode.Open))
            {
                var tagFile = TagLib.File.Create(new StreamFileAbstraction(fileStream.Name,
                    fileStream, fileStream));


                tagFile.Tag.Album = model.Album;
                tagFile.Tag.AlbumArtists = new[] {model.AlbumArtist};
                tagFile.Tag.Performers = new[] {model.Artist};
                tagFile.Tag.Title = model.Title;
                tagFile.Tag.Comment = model.Comment;
                tagFile.Tag.Genres = new[] { model.Genre };
                tagFile.Tag.Year = model.Year;
                tagFile.Save();
            }

            File.Move(model.FilePath, model.TargetFilePath);
        }
    }
}
