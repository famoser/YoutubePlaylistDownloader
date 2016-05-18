using System;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Data
{
    public class Mp3FileMetaData
    {
        /// <summary>
        /// Youtube Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Version of the programm
        /// </summary>
        public int CreatedProgramVersion { get; set; }

        /// <summary>
        /// Version of the programm
        /// </summary>
        public int SaveProgramVersion { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime SaveDate { get; set; }
    }
}
