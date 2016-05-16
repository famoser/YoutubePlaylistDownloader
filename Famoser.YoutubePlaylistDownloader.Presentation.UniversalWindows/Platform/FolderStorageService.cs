using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Famoser.FrameworkEssentials.Logging;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using UniversalEssentials.Platform;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform
{
    public class FolderStorageService : StorageService, IFolderStorageService
    {
        public async Task<List<string>> GetAllFilesFromFolder(FolderType type, string folder)
        {
            try
            {
                if (type == FolderType.Music)
                {
                    var files = await (await KnownFolders.MusicLibrary.GetFolderAsync(folder)).GetFilesAsync();

                    return files.Select(storageFile => storageFile.Name).ToList();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        public async Task<Stream> GetFile(FolderType type, string path, string fileName)
        {
            try
            {
                if (type == FolderType.Music)
                {
                    StorageFile storageFile = await (await KnownFolders.MusicLibrary.GetFolderAsync(path)).GetFileAsync(Path.Combine(path, fileName));

                    var randomAccessStream = await storageFile.OpenReadAsync();
                    return randomAccessStream.AsStreamForRead();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return null;
        }

        public async Task<bool> DeleteFiles(FolderType type, string folder, List<string> files)
        {
            try
            {
                if (type == FolderType.Music)
                {
                    var storageFiles = await (await KnownFolders.MusicLibrary.GetFolderAsync(folder)).GetFilesAsync();
                    foreach (var storageFile in storageFiles)
                    {
                        if (files.Any(f => f == storageFile.Name))
                            await storageFile.DeleteAsync();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<bool> SaveFile(FolderType type, string path, string filename, Stream stream)
        {
            try
            {
                if (type == FolderType.Music)
                {
                    await KnownFolders.MusicLibrary.CreateFolderAsync(path);

                    var si = await (await KnownFolders.MusicLibrary.GetFolderAsync(path)).CreateFileAsync(Path.Combine(path, filename),
                            CreationCollisionOption.ReplaceExisting);

                    await FileIO.WriteBytesAsync(si, StreamHelper.StreamToByte(stream));
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex, this);
            }
            return false;
        }
    }
}
