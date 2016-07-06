using System.Threading.Tasks;
using Famoser.YoutubePlaylistDownloader.Business.Models;
using Famoser.YoutubePlaylistDownloader.Business.Repositories.Interfaces;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
namespace Famoser.YoutubePlaylistDownloader.Business.Repositories.Mock
{
    public class SmartRepositoryMock : ISmartRepository
    {
        public async Task<bool> FillAutomaticProperties(Mp3Model model)
        {
            return true;
        }
    }
}
