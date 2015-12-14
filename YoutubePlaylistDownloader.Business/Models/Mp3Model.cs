﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubePlaylistDownloader.Business.Models
{
    public class Mp3Model : BaseModel
    {
        private string _originalTitle;
        public string OriginalTitle
        {
            get { return _originalTitle; }
            set { Set(ref _originalTitle, value); }
        }

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

        private string _comment;
        public string Comment
        {
            get { return _comment; }
            set { Set(ref _comment, value); }
        }

        public string FilePath { get; set; }
        public string TargetFolder { get; set; }
        public string TargetFilePath
        {
            get { return Path.Combine(TargetFolder, Artist + " - " + Title + ".mp3"); }
        }
    }
}
