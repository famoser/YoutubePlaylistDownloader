﻿using System.Collections.Generic;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class SaveModel
    {
        public string TargetFolder { get; set; }
        public string TempFolder { get; set; }

        public List<string> SelectedPlaylists { get; set; } 
    }
}
