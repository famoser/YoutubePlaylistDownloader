using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IFolderStorageService : IStorageService
    {
        Task<List<string>> GetAllFilesFromFolder(FolderType type, string folder);

        Task<Stream> GetFile(FolderType type, string path, string fileName);

        Task<bool> SaveFile(FolderType type, string path, string filename, Stream stream);

        Task<bool> DeleteFiles(FolderType type, string folder, List<string> files);
    }
}
