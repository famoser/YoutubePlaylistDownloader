using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Data
{
    public class Mp3FileMetaData
    {
        /// <summary>
        /// Version of the programm
        /// </summary>
        public int V { get; set; }

        /// <summary>
        /// Youtube Id
        /// </summary>
        public string Id { get; set; }
    }
}
