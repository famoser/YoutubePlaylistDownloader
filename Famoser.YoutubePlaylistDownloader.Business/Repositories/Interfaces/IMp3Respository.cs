using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IMp3Respository
    {
        Task<bool> LoadFile(Mp3Model model);
        Task<bool> SaveFile(Mp3Model model);
        Task<bool> CreateFile(VideoModel video, Stream fileStream);
    }
}
