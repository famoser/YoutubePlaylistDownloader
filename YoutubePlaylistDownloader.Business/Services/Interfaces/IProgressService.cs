using YoutubePlaylistDownloader.Business.Models;

namespace YoutubePlaylistDownloader.Business.Services.Interfaces
{
    public interface IProgressService
    {
        void SetProgress(ProgressModel pm, int progress);
        void RemoveProgress(ProgressModel pm);
    }
}
