using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Save
{
    public class PlaylistCacheModel
    {
        public string Id { get; set; }
        
        public bool Download { get; set; }

        public List<string> DownloadedFiles { get; set; }

        public List<string> FailedFiles { get; set; }
    }
}
