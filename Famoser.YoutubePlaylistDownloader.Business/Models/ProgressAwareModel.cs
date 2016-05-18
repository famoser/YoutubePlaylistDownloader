using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Famoser.FrameworkEssentials.Services;

namespace Famoser.YoutubePlaylistDownloader.Business.Models
{
    public class ProgressAwareModel : BaseModel
    {
        private ProgressService _progressServie;
        public ProgressService ProgressServie
        {
            get { return _progressServie; }
            set { Set(ref _progressServie, value); }
        }
    }
}
