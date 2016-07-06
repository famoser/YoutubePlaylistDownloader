using System;
using System.IO;
using System.Linq;
using Famoser.YoutubePlaylistDownloader.Business.Models.Data;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class Mp3Model : BaseModel
    {
        #region editable fields
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

        private uint _track;
        public uint Track
        {
            get { return _track; }
            set { Set(ref _track, value); }
        }

        private uint _trackCount;
        public uint TrackCount
        {
            get { return _trackCount; }
            set { Set(ref _trackCount, value); }
        }
        #endregion

        private byte[] _albumCover;
        public byte[] AlbumCover
        {
            get { return _albumCover; }
            set { Set(ref _albumCover, value); }
        }

        private Mp3FileInfo _fileInfo;
        public Mp3FileInfo FileInfo
        {
            get { return _fileInfo; }
            set { Set(ref _fileInfo, value); }
        }

        public string FilePath { get; set; }

        public VideoModel VideoModel { get; set; }

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
