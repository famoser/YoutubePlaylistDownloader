using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface ISmartRepository
    {
        Task<Uri> GetAlbumCoverUri(Mp3Model model);
        void AssignMetaTags(PlaylistModel list);
        void AssignMetaTags(IList<PlaylistModel> list);
    }
}
