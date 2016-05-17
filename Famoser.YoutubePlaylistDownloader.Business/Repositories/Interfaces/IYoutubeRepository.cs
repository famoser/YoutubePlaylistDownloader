using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IYoutubeRepository
    {
        Task<ObservableCollection<PlaylistModel>> GetPlaylists();
        Task<bool> DownloadVideos(IProgressService progressService);
        Task<PlaylistModel> GetPlaylistByLink(string link);
    }
}
