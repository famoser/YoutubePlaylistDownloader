namespace Famoser.YoutubePlaylistDownloader.Business.Enums
{
    public enum SaveStatus
    {
        Discovered = 0,
        DownloadPending = 10,
        Downloading = 11,
        Downloaded = 12,
        Converting = 13,
        Converted = 14,
        FillingAutomaticProperties = 15,
        FilledAutomaticProperties = 16,
        Saving = 17,
        Saved = 18,
        Finished = 20,

        FailedDownloadingResolvingUrl = 1111,
        FailedDownloading = 1110,
        FailedConverting = 1120,
        FailedDownloadOrConversion = 1666,
        FailedFillingAutomaticProperties = 1150,
        FailedSaving = 1130
    }
}
