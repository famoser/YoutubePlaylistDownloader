using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IVideoRespository
    {
        Task<bool> LoadFromMusicLibrary(VideoModel model);
        Task<bool> SaveToMusicLibrary(VideoModel model);
        Task<bool> CreateToMusicLibrary(VideoModel video, Stream fileStream);
    }
}
