using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers.Converters
{
   public class Mp3Converter : BaseConverter<Mp3CacheModel, Mp3Model>
    {
       public override Mp3CacheModel Convert(Mp3Model model)
       {
           return new Mp3CacheModel()
           {
               Album = model.Album,
               AlbumArtist = model.AlbumArtist,
               Artist = model.Artist,
               Genre = model.Genre,
               FilePath = model.FilePath,
               Title = model.Title,
               Year = model.Year
           };
       }

       public override Mp3Model Convert(Mp3CacheModel model)
       {
            return new Mp3Model()
            {
                Album = model.Album,
                AlbumArtist = model.AlbumArtist,
                Artist = model.Artist,
                Genre = model.Genre,
                FilePath = model.FilePath,
                Title = model.Title,
                Year = model.Year
            };
       }
    }
}
