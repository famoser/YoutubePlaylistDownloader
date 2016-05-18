using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using File = TagLib.File;

namespace Famoser.YoutubePlaylistDownloader.Presentation.UniversalWindows.Platform
{
    public class StreamFileAbstraction : File.IFileAbstraction
    {
        private StorageFile _file;
        private Stream _fileStream;
        private Stream _writeStream;

        public StreamFileAbstraction(StorageFile file)
        {
            _file = file;
        }

        public async Task Initialize()
        {
            _fileStream = (await _file.OpenReadAsync()).AsStreamForRead();

            var stream = new MemoryStream();
            await _fileStream.CopyToAsync(stream);
            _writeStream = stream;
        }

        public void CloseStream(Stream stream)
        {

        }

        public string Name => _file.Name;
        public Stream ReadStream => _fileStream;
        public Stream WriteStream => _writeStream;

        public StorageFile File => _file;
    }
}
