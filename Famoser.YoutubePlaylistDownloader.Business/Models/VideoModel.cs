﻿using Famoser.YoutubePlaylistDownloader.Business.Enums;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class VideoModel : YoutubeModel
    {
        public override string Link => "https://www.youtube.com/watch?v=" + Id;

        private Mp3Model _mp3Model;
        public Mp3Model Mp3Model
        {
            get { return _mp3Model; }
            set { Set(ref _mp3Model, value); }
        }

        private SaveStatus _saveStatus;
        public SaveStatus SaveStatus
        {
            get { return _saveStatus; }
            set { Set(ref _saveStatus, value); }
        }

        private int _trackNumberInPlaylist;
        public int TrackNumberInPlaylist
        {
            get { return _trackNumberInPlaylist; }
            set { Set(ref _trackNumberInPlaylist, value); }
        }

        public PlaylistModel PlaylistModel { get; set; }
    }
}
