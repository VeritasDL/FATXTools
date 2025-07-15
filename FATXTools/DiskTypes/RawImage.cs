using System.IO;

namespace FATXTools.DiskTypes
{
    public class RawImage
    {
        private readonly FileStream _fileStream;
        private readonly FATX.DriveReader _reader;
        private readonly FATX.DriveWriter _writer;

        public RawImage(string fileName)
        {
            _fileStream = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite);
            _reader = new FATX.DriveReader(_fileStream, _writer);
            _reader.Initialize();
            //_writer.Initialize();
        }

        public FATX.DriveReader Reader => _reader;
        public FATX.DriveWriter Writer => _writer;
        public FileStream FileStream => _fileStream;
    }
}