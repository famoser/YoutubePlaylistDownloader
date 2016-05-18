using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Famoser.YoutubePlaylistDownloader.Business.Attributes
{
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; private set; }

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}
