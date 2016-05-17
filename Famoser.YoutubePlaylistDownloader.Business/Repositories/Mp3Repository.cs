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
using TagLib;
using TagLib.Id3v2;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories
{
    public class Mp3Repository : IMp3Respository
    {
        private readonly IFolderStorageService _folderStorageService;
        private readonly ISettingsRepository _settingsRepository;
        private static readonly FolderType _type = FolderType.Music;
        private static readonly string subFolder = "youtube";

        public Mp3Repository(IFolderStorageService folderStorageService, ISettingsRepository settingsRepository)
        {
            _folderStorageService = folderStorageService;
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// disabled
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        public async Task<List<Mp3Model>> GetModelsForPlaylist(PlaylistModel playlist, IProgressService service)
        {
            var files = await _folderStorageService.GetAllFilesFromFolder(_type, Path.Combine(subFolder, playlist.Id));
            var res = new List<Mp3Model>();
            foreach (var file in files)
            {
                var fileStream = await _folderStorageService.GetFile(_type, playlist.Id, file);
                var tagFile = File.Create(new StreamFileAbstraction(file,
                    fileStream, fileStream));

                var tags = (TagLib.Id3v2.Tag)tagFile.GetTag(TagTypes.Id3v2);
                PrivateFrame p = PrivateFrame.Get(tags, "CustomKey", true);
                p.PrivateData = Encoding.Unicode.GetBytes("Sample Value");
                var model = new Mp3Model()
                {
                    Album = tags.Album,
                    AlbumArtist = tags.AlbumArtists.FirstOrDefault(),
                    Artist = tags.Performers.FirstOrDefault(),
                    Comment = tags.Comment,
                    Genre = tags.Genres.FirstOrDefault(),
                    Title = tags.Title,
                    Year = tags.Year,
                    Mp3File = tagFile
                };
                res.Add(model);
            }
            return res;
        }

        public async Task<bool> SavePlaylists(IList<PlaylistModel> playlists)
        {
            try
            {
                foreach (var playlist in playlists)
                {
                    foreach (var mp3Model in playlist.Videos)
                    {
                        if (mp3Model.Mp3File == null)
                        {
                            var fileStream = await _folderStorageService.GetFile(_type, playlist.Id, mp3Model.GetRecommendedFileName());
                            var tagFile = File.Create(new StreamFileAbstraction(mp3Model.GetRecommendedFileName(), fileStream, fileStream));
                            mp3Model.Mp3File = tagFile;
                        }

                        //to avoid null reference exception
                        if (mp3Model.Genre == null)
                            mp3Model.Genre = "";
                        if (mp3Model.AlbumArtist == null)
                            mp3Model.AlbumArtist = "";
                        if (mp3Model.Artist == null)
                            mp3Model.Artist = "";

                        mp3Model.Mp3File.Tag.Album = mp3Model.Album;
                        mp3Model.Mp3File.Tag.AlbumArtists = new[] { mp3Model.AlbumArtist };
                        mp3Model.Mp3File.Tag.Performers = new[] { mp3Model.Artist };
                        mp3Model.Mp3File.Tag.Title = mp3Model.Title;
                        mp3Model.Mp3File.Tag.Comment = mp3Model.Comment;
                        mp3Model.Mp3File.Tag.Genres = new[] { mp3Model.Genre };
                        mp3Model.Mp3File.Tag.Year = mp3Model.Year;
                        if (mp3Model.AlbumCover != null)
                        {
                            var picture = await DownloadHelper.GetAlbumArt(mp3Model.AlbumCover);
                            if (picture != null)
                            {
                                mp3Model.Mp3File.Tag.Pictures = new IPicture[1] { picture };
                            }
                        }
                        mp3Model.Mp3File.Save();
                    }

                    playlist.DownloadedVideos.AddRange(playlist.Videos.Select(e => e.VideoInfo));
                    playlist.Videos.Clear();
                }

                var cacheModel = new CacheModel()
                {
                    CachedPlaylists = ConverterHelper.Convert(playlists)
                };
                await _settingsRepository.SaveCache(cacheModel);


                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.LogException(ex);
            }
            return false;
        }
    }
}
