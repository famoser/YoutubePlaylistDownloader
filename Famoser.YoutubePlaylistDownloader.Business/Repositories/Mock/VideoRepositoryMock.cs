using System.IO;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Mock
{
    public class VideoRepositoryMock : IVideoRespository
    {
        public async Task<bool> LoadFromMusicLibrary(VideoModel model)
        {
            return true;
        }

        public async Task<bool> SaveToMusicLibrary(VideoModel model)
        {
            return true;
        }

        public async Task<bool> CreateToMusicLibrary(VideoModel video, Stream fileStream)
        {
            return true;
        }
    }
}
