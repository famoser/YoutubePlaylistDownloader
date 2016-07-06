using Famoser.FrameworkEssentials.Services;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class ProgressAwareModel : BaseModel
    {
        public ProgressAwareModel()
        {
            ProgressService = new ProgressService();
        }

        public ProgressService ProgressService { get; }
    }
}
