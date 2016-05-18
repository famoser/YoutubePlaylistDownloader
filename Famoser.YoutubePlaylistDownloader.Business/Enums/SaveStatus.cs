using Famoser.YoutubePlaylistDownloader.Business.Attributes;

namespace Famoser.YoutubePlaylistDownloader.Business.Enums
{
    public enum SaveStatus
    {
        [Description("discovered")]
        Discovered = 0,

        [Description("download pending")]
        DownloadPending = 10,

        [Description("downloading...")]
        Downloading = 11,

        [Description("downloaded")]
        Downloaded = 12,

        [Description("converting...")]
        Converting = 13,

        [Description("converted")]
        Converted = 14,

        [Description("filling properties...")]
        FillingAutomaticProperties = 15,

        [Description("filled properties")]
        FilledAutomaticProperties = 16,

        [Description("saving...")]
        Saving = 17,

        [Description("saved")]
        Saved = 18,

        [Description("finished")]
        Finished = 20,




        [Description("downloading...")]
        FailedDownloadingResolvingUrl = 1111,

        [Description("downloading...")]
        FailedDownloading = 1110,

        [Description("downloading...")]
        FailedConverting = 1120,

        [Description("downloading...")]
        FailedDownloadOrConversion = 1666,

        [Description("downloading...")]
        FailedFillingAutomaticProperties = 1150,

        [Description("downloading...")]
        FailedSaving = 1130
    }
}
