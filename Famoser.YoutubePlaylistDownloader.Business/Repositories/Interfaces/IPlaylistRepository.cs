using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        ObservableCollection<PlaylistModel> GetPlaylists();
        Task<bool> RefreshPlaylist(PlaylistModel playlist);
        Task<bool> DownloadVideosForPlaylist(PlaylistModel playlist);
        Task<bool> RefreshAllPlaylists();
        Task<bool> DownloadVideosForAllPlaylists();
        Task<bool> AddNewPlaylistByLink(string link);
    }
}
