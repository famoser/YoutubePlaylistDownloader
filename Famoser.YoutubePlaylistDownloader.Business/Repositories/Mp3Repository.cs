using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Save;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Newtonsoft.Json;
using TagLib;
using TagLib.Id3v2;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class Mp3Repository : IMp3Respository
    {
        private readonly IFolderStorageService _folderStorageService;
        private static readonly FolderType Type = FolderType.Music;
        private static readonly string SubFolder = "youtube";

        public Mp3Repository(IFolderStorageService folderStorageService)
        {
            _folderStorageService = folderStorageService;
        }
        
        public async Task<bool> LoadFile(Mp3Model model)
        {
            try
            {
                var fileStream = await _folderStorageService.GetFile(Type, model.SavePath);
                var tagFile = File.Create(new StreamFileAbstraction(model.SavePath,
                    fileStream, fileStream));

                model.Title = tagFile.Tag.Title;
                model.Album = tagFile.Tag.Album;
                model.Artist = string.Join(", ", tagFile.Tag.Performers);
                model.AlbumArtist = string.Join(", ", tagFile.Tag.AlbumArtists);
                model.Genre = string.Join(", ", tagFile.Tag.Genres);
                model.Year = tagFile.Tag.Year;

                if (tagFile.Tag.Pictures != null)
                {
                    var pic = tagFile.Tag.Pictures.FirstOrDefault();
                    model.AlbumCover = pic.Data.Data;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<bool> SaveFile(Mp3Model model)
        {
            try
            {
                //todo: move file if name changed
                var fileStream = await _folderStorageService.GetFile(Type, model.SavePath);
                var tagFile = File.Create(new StreamFileAbstraction(model.SavePath, fileStream, fileStream));
                
                //save all tags
                tagFile.Tag.Album = model.Album;
                tagFile.Tag.Title = model.Title;
                tagFile.Tag.AlbumArtists = model.AlbumArtist.Split(new []{", "}, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Performers = model.Artist.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Genres = model.Genre.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Year = model.Year;

                //save image
                if (model.AlbumCover != null && model.AlbumCover.Length > 0)
                {
                    var vektor = new ByteVector(model.AlbumCover);
                    IPicture newArt = new Picture(vektor);
                    tagFile.Tag.Pictures = new IPicture[1] { newArt };
                }
                else
                {
                    tagFile.Tag.Pictures = new IPicture[0];
                }

                // save meta data
                var id3Tag = tagFile.GetTag(TagTypes.Id3v2);
                if (id3Tag is TagLib.Id3v2.Tag)
                {
                    PrivateFrame mp3Json = PrivateFrame.Get(id3Tag as TagLib.Id3v2.Tag, "ypdjson", true);
                    mp3Json.PrivateData = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Mp3Helper.GetMp3FileMetaData(model)));
                }

                tagFile.Save();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }

        public async Task<bool> CreateFile(VideoModel video, Stream fileStream)
        {
            try
            {
                video.SaveStatus = SaveStatus.Saving;

                var mp3Model = new Mp3Model()
                {
                    Title = video.Name,
                    VideoModel = video
                };

                var filePath = GetRecommendedFilePath(mp3Model);
                if (await _folderStorageService.SaveFile(Type, filePath, fileStream))
                {
                    mp3Model.SavePath = filePath;
                    mp3Model.VideoModel = video;
                    video.Mp3Model = mp3Model;
                    video.SaveStatus = SaveStatus.Saved;
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
                video.SaveStatus = SaveStatus.FailedSaving;
            }
            return false;
        }

        private string GetRecommendedFilePath(Mp3Model model)
        {
            return Path.Combine(SubFolder, model.VideoModel.PlaylistModel.Id, model.GetRecommendedFileName());
        }
    }
}
