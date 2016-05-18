using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using TagLib;

namespace Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IFolderStorageService : IStorageService
    {
        Task<Stream> GetFile(FolderType type, string path);
        Task<File.IFileAbstraction> GetTagLibFile(FolderType type, string path);
        Task<bool> SaveTagLibFile(File.IFileAbstraction abstraction);
        Task<bool> SaveFile(FolderType type, string path, Stream stream);
        Task<bool> MoveFile(FolderType type, string path, string newPath);

        Task<List<string>> GetAllFilesFromFolder(FolderType type, string folder);
        Task<bool> DeleteFilesInFolder(FolderType type, string path, List<string> files);
    }
}
