using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Logging;
using Famoser.FrameworkEssentials.Services.Base;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Helpers;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Data;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Services.Interfaces;
using Newtonsoft.Json;
using TagLib;
using TagLib.Id3v2;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class VideoRepository : BaseService, IVideoRespository
    {
        private readonly IFolderStorageService _folderStorageService;
        private readonly ISettingsRepository _settingsRepository;
        private static readonly FolderType Type = FolderType.Music;
        private static readonly string SubFolder = "youtube";

        public VideoRepository(IFolderStorageService folderStorageService, ISettingsRepository settingsRepository) : base(true, LogHelper.Instance)
        {
            _folderStorageService = folderStorageService;
            _settingsRepository = settingsRepository;
        }


        public Task<bool> LoadFromMusicLibrary(VideoModel videoModel)
        {
            return Execute(async () =>
            {
                var model = videoModel.Mp3Model;
                var file = await _folderStorageService.GetTagLibFile(Type, model.FilePath);
                var tagFile = File.Create(file);

                model.Title = tagFile.Tag.Title;
                model.Album = tagFile.Tag.Album;
                model.Artist = string.Join(", ", tagFile.Tag.Performers);
                model.AlbumArtist = string.Join(", ", tagFile.Tag.AlbumArtists);
                model.Genre = string.Join(", ", tagFile.Tag.Genres);
                model.Year = tagFile.Tag.Year;

                model.Track = tagFile.Tag.Track;
                model.TrackCount = tagFile.Tag.TrackCount;

                model.FileInfo = new Mp3FileInfo
                {
                    Duration = tagFile.Properties.Duration,
                    AudioBitrate = tagFile.Properties.AudioBitrate
                };

                // save meta data
                var id3Tag = tagFile.GetTag(TagTypes.Id3v2);
                if (id3Tag is TagLib.Id3v2.Tag)
                {
                    try
                    {
                        PrivateFrame mp3Json = PrivateFrame.Get(id3Tag as TagLib.Id3v2.Tag, "ypdjson", false);
                        var json = Encoding.Unicode.GetString(mp3Json.PrivateData.Data, 0,
                            mp3Json.PrivateData.Data.Length);
                        var metaData = JsonConvert.DeserializeObject<Mp3FileMetaData>(json);
                        model.FileInfo.SaveDate = metaData.SaveDate;
                        model.FileInfo.SaveProgramVersion = metaData.SaveProgramVersion;
                        model.FileInfo.CreatedProgramVersion = metaData.CreatedProgramVersion;
                        model.FileInfo.CreateDate = metaData.CreateDate;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.LogException(ex);
                    }
                }

                var pic = tagFile.Tag.Pictures?.FirstOrDefault();
                if (pic != null)
                    model.AlbumCover = pic.Data.Data;

                return true;
            });
        }

        public Task<bool> SaveToMusicLibrary(VideoModel videoModel)
        {
            return Execute(async () =>
            {
                var model = videoModel.Mp3Model;
                var file = await _folderStorageService.GetTagLibFile(Type, model.FilePath);
                var tagFile = File.Create(file);

                //save all tags
                tagFile.Tag.Album = model.Album;
                tagFile.Tag.Title = model.Title;
                tagFile.Tag.AlbumArtists = model.AlbumArtist.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Performers = model.Artist.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Genres = model.Genre.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                tagFile.Tag.Year = model.Year;

                //automatic
                tagFile.Tag.Disc = 1;
                tagFile.Tag.DiscCount = 1;
                tagFile.Tag.Track = model.Track;
                tagFile.Tag.TrackCount = model.TrackCount;

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
                    mp3Json.PrivateData =
                        Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(Mp3Helper.GetMp3FileMetaData(model)));
                }

                tagFile.Save();
                await _folderStorageService.SaveTagLibFile(file);

                var recommendedFilePath = GetRecommendedFilePath(model);
                if (model.FilePath != recommendedFilePath)
                {
                    await _folderStorageService.MoveFile(FolderType.Music, model.FilePath, recommendedFilePath);
                    model.FilePath = recommendedFilePath;
                    await _settingsRepository.SaveCache();
                }
                return true;
            });
        }

        public Task<bool> CreateToMusicLibrary(VideoModel video, Stream fileStream)
        {
            return Execute(async () =>
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
                    mp3Model.FilePath = filePath;
                    mp3Model.VideoModel = video;
                    video.Mp3Model = mp3Model;
                    video.SaveStatus = SaveStatus.Saved;
                    return true;
                }
                return false;
            });
        }

        private string GetRecommendedFilePath(Mp3Model model)
        {
            return Path.Combine(SubFolder, model.VideoModel.PlaylistModel.Name, model.GetRecommendedFileName());
        }
    }
}
