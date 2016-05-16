using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services.Interfaces;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface IMp3Respository
    {
        Task<List<Mp3Model>> GetModelsForPlaylist(PlaylistModel playlist, IProgressService service);
        Task<bool> SaveModelsOfPlaylist(PlaylistModel playlist, List<Mp3Model> models);
    }
}
