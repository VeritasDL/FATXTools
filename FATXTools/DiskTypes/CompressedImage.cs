using System.IO;

namespace FATXTools.DiskTypes
{
    public class CompressedImage : FATX.DriveReader
    {
        public CompressedImage(string fileName)
            : base(new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
        {
            // Verify IMGC header

            // Pre-load blocks

            //base.Initialize();
        }
    }
}
