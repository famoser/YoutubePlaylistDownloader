using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<ObservableCollection<PlaylistModel>> GetPlaylists();
        Task<bool> RefreshPlaylist(PlaylistModel playlist, IProgressService progressService);
        Task<bool> DownloadVideosForPlaylist(PlaylistModel playlist, IProgressService progressService);
        Task<bool> RefreshAllPlaylists(IProgressService progressService);
        Task<bool> DownloadVideosForAllPlaylists(IProgressService progressService);
        Task<bool> AddNewPlaylistByLink(string link);

        ObservableCollection<PlaylistModel> GetDesignCollection();
    }
}
