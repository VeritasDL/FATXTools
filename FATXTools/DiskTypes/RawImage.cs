using System.IO;

namespace FATXTools.DiskTypes
{
    public class RawImage : FATX.DriveReader
    {
        // TODO: replace with FileStream to be able to use "using"
        public RawImage(string fileName, bool write = true)
            : base(new FileStream(fileName, FileMode.Open, write ? FileAccess.ReadWrite : FileAccess.Read))
        {
            base.Initialize();
        }
    }
}
