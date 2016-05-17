using System;
using System.IO;
using System.Linq;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class Mp3Model : BaseModel
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        private string _artist;
        public string Artist
        {
            get { return _artist; }
            set { Set(ref _artist, value); }
        }

        private string _albumArtist;
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { Set(ref _albumArtist, value); }
        }

        private string _album;
        public string Album
        {
            get { return _album; }
            set { Set(ref _album, value); }
        }

        private string _genre;
        public string Genre
        {
            get { return _genre; }
            set { Set(ref _genre, value); }
        }

        private uint _year;
        public uint Year
        {
            get { return _year; }
            set { Set(ref _year, value); }
        }

        private byte[] _albumCover;
        public byte[] AlbumCover
        {
            get { return _albumCover; }
            set { Set(ref _albumCover, value); }
        }

        public string SavePath { get; set; }

        public VideoModel VideoModel { get; set; }

        public bool AllImportantPropertiesFilled => !string.IsNullOrEmpty(Title) &&
                                                    !string.IsNullOrEmpty(Artist) &&
                                                    !string.IsNullOrEmpty(Album) &&
                                                    !string.IsNullOrEmpty(AlbumArtist) &&
                                                    !string.IsNullOrEmpty(Genre);

        public string GetRecommendedFileName()
        {
            var fileName = Guid.NewGuid().ToString();
            if (!string.IsNullOrEmpty(Title))
                if (!string.IsNullOrEmpty(Artist))
                    fileName = Artist + " - " + Title;
                else
                    fileName = Title;

            fileName += ".mp3";

            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
    }
}
