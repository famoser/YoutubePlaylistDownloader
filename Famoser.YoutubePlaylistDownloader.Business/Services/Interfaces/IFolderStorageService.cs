using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IFolderStorageService : IStorageService
    {
        Task<Stream> GetFile(FolderType type, string path);
        Task<bool> SaveFile(FolderType type, string path, Stream stream);
        Task<bool> MoveFile(FolderType type, string path, string newPath);

        Task<List<string>> GetAllFilesFromFolder(FolderType type, string folder);
        Task<bool> DeleteFilesInFolder(FolderType type, string path, List<string> files);
    }
}
