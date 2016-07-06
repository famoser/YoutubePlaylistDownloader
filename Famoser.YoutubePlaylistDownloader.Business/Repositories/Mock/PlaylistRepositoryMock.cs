using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Enums;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Models.Data;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Mock
{
    public class PlaylistRepositoryMock  : IPlaylistRepository
    {
        public ObservableCollection<PlaylistModel> GetPlaylists()
        {
            return new ObservableCollection<PlaylistModel>()
            {
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(false),
                GetExamplePlaylist(false),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
                GetExamplePlaylist(),
            };
        }

        public async Task<bool> RefreshPlaylist(PlaylistModel playlist, IProgressService progressService)
        {
            return true;
        }

        public async Task<bool> DownloadVideosForPlaylist(PlaylistModel playlist, IProgressService progressService)
        {
            return true;
        }

        public async Task<bool> RefreshAllPlaylists(IProgressService progressService)
        {
            return true;
        }

        public async Task<bool> DownloadVideosForAllPlaylists(IProgressService progressService)
        {
            return true;
        }

        public async Task<bool> AddNewPlaylistByLink(string link)
        {
            return true;
        }


        private PlaylistModel GetExamplePlaylist(bool refresh = true)
        {
            var playlist = new PlaylistModel()
            {
                Name = "Music playlist",
                Id = "ajdkga8rldhs7",
                Refresh = refresh,
                Videos = new ObservableCollection<VideoModel>()
                {
                    GetExampleVideo(SaveStatus.Discovered),
                    GetExampleVideo(SaveStatus.DownloadPending),
                    GetExampleVideo(SaveStatus.Downloading),
                    GetExampleVideo(SaveStatus.Downloaded),
                    GetExampleVideo(SaveStatus.Converting),
                    GetExampleVideo(SaveStatus.Converted),
                    GetExampleVideo(SaveStatus.FillingAutomaticProperties),
                    GetExampleVideo(SaveStatus.FilledAutomaticProperties),
                    GetExampleVideo(SaveStatus.Saving),
                    GetExampleVideo(SaveStatus.Saved),
                    GetExampleVideo(),
                    GetExampleVideo(),
                    GetExampleVideo(),
                    GetExampleVideo(SaveStatus.FailedDownloadOrConversion)
                }
            };
            foreach (var videoModel in playlist.Videos)
            {
                videoModel.PlaylistModel = playlist;
            }
            return playlist;
        }

        private VideoModel GetExampleVideo(SaveStatus status = SaveStatus.Finished)
        {
            var vm = new VideoModel()
            {
                Id = "hja7wdjka7ef8af6asdf6",
                Mp3Model = GetExampleMp3(),
                Name = "Video name - Alle farben (" + status + ")",
                SaveStatus = status
            };
            vm.Mp3Model.VideoModel = vm;
            if (vm.SaveStatus < SaveStatus.Finished)
            {
                vm.ProgressService = new ProgressService();
                vm.ProgressService.ConfigurePercentageProgress(100, 20);
            }
            return vm;
        }

        private Mp3Model GetExampleMp3()
        {
            var mp3 = new Mp3Model()
            {
                Album = "Album alle Farben",
                AlbumArtist = "Artists für Album",
                Artist = "Artists für President",
                Genre = "Genre des Dinos",
                Title = "Grösser Titer",
                Year = 2014,
                FilePath = "8213ää123ö12ü713äa/Artist - Song.mp3",
                FileInfo = GetInfoExampleModel()
            };
            return mp3;
        }

        private Mp3FileInfo GetInfoExampleModel()
        {
            return new Mp3FileInfo()
            {
                SaveDate = DateTime.Now,
                CreatedProgramVersion = 1,
                CreateDate = DateTime.Now,
                SaveProgramVersion = 2,
                AudioBitrate = 200,
                Duration = TimeSpan.FromSeconds(200)
            };
        }
    }
}
