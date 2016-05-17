using System.Collections.Generic;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IMp3Respository
    {
        Task<bool> SavePlaylists(IList<PlaylistModel> playlists);
    }
}
