using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Famoser.YoutubePlaylistDownloader.Business.Models.Data
{
    public class Mp3FileInfo : BaseModel
    {
        private DateTime _createDate;
        public DateTime CreateDate
        {
            get { return _createDate; }
            set { Set(ref _createDate, value); }
        }

        private DateTime _saveDate;
        public DateTime SaveDate
        {
            get { return _saveDate; }
            set { Set(ref _saveDate, value); }
        }

        private int _createdProgramVersion;
        public int CreatedProgramVersion
        {
            get { return _createdProgramVersion; }
            set { Set(ref _createdProgramVersion, value); }
        }

        private int _saveProgramVersion;
        public int SaveProgramVersion
        {
            get { return _saveProgramVersion; }
            set { Set(ref _saveProgramVersion, value); }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return _duration; }
            set { Set(ref _duration, value); }
        }

        private int _audioBitrate;
        public int AudioBitrate
        {
            get { return _audioBitrate; }
            set { Set(ref _audioBitrate, value); }
        }
    }
}
