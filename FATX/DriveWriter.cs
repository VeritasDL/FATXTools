using System.IO;
using System.Collections.Generic;

namespace FATX
{
    public class DriveWriter : EndianWriter
    {
        private List<FileSystem.Volume> _partitions = new List<FileSystem.Volume>();

        public DriveWriter(Stream stream)
            : base(stream)
        {
        }

        public void Initialize()
        {
        }
        public void AddPartition(FileSystem.Volume partition)
        {
            _partitions.Add(partition);
        }
        public FileSystem.Volume GetPartition(int index)
        {
            return _partitions[index];
        }
        public List<FileSystem.Volume> Partitions => _partitions;
    }
}