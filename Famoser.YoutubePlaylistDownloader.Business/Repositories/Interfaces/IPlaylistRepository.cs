using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<ObservableCollection<PlaylistModel>> GetPlaylists();
        Task<bool> RefreshPlaylists(IProgressService progressService);
        Task<bool> DownloadVideos(IProgressService progressService);
        Task<bool> AddNewPlaylistByLink(string link);
    }
}
