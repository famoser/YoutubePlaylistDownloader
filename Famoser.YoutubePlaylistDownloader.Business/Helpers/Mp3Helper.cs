using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Data;

namespace Famoser.YoutubePlaylistDownloader.Business.Helpers
{
    public class Mp3Helper
    {
        private const int ProgrammVersion = 1;

        public static Mp3FileMetaData GetMp3FileMetaData(Mp3Model model)
        {
            return new Mp3FileMetaData()
            {
                Id = model.VideoModel.Id,
                V = ProgrammVersion
            };
        }
    }
}
