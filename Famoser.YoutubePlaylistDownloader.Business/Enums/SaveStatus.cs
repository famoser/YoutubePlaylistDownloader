namespace Famoser.YoutubePlaylistDownloader.Business.Enums
{
    public enum SaveStatus
    {
        Discovered = 0,
        DownloadPending = 10,
        Downloading = 11,
        Converting = 12,
        Saving = 13,
        Finished = 20,

        FailedDownloading = 111,
        FailedConverting = 112,
        FailedSaving = 113
    }
}
