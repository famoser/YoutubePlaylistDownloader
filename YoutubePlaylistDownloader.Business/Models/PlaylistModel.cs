using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlaylistDownloader.Business.Models
{
    public class PlaylistModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool Download { get; set; }

        public int TotalVideos { get; set; }
    }
}
