using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;

namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces
{
    public interface ISmartRepository
    {
        Task<bool> FillAutomaticProperties(Mp3Model model);
    }
}
