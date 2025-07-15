using FATX.Analyzers;
using System;
using System.Collections.Generic;
using System.IO;

namespace FATX.FileSystem
{

    public class Volume
    {
        private readonly DriveReader _reader;
        private readonly DriveWriter _writer;
        private readonly string _partitionName;
        private readonly long _partitionOffset;
        private long _partitionLength;
        public const uint VolumeSignature = 0x58544146;

        private uint _signature;
        private uint _serialNumber;
        private uint _sectorsPerCluster;
        private uint _rootDirFirstCluster;

        private uint _bytesPerCluster;
        private uint _maxClusters;
        private uint _bytesPerFat;
        private bool _isFat16;
        private uint _fatByteOffset;
        private uint _fileAreaByteOffset;

        private bool _usesLegacyFormat;
        private uint _jump;
        private ushort _bytesPerSector;
        private ushort _reservedSectors;
        private ushort _sectorsPerTrack;
        private ushort _heads;
        private uint _hiddenSectors;
        private uint _largeSectors;
        private uint _largeSectorsPerFat;

        private List<DirectoryEntry> _root = new List<DirectoryEntry>();
        private uint[] _fileAllocationTable;
        private long _fileAreaLength;
        private Platform _platform;
        /// <summary>
        /// Creates a new FATX volume.
        /// </summary>
        /// <param name="reader">The stream that contains the FATX file system.</param>
        /// <param name="name">The name of this volume.</param>
        /// <param name="offset">The offset within the <paramref name="reader"/> this partition exists at.</param>
        /// <param name="length">The length in bytes of this partition.</param>
        /// <param name="legacy">
        /// Set this to true if this is the legacy FATX file system. 
        /// It appears to only be used in older DVT3 xbox systems with kernel 3633 or earlier.
        /// </param>
        public Volume(DriveReader reader, DriveWriter writer, string name, long offset, long length, bool legacy = false)
        {
            this._reader = reader;
            this._writer = writer;
            this._partitionName = name;
            this._partitionLength = length;
            this._partitionOffset = offset;
            this._usesLegacyFormat = legacy;
            this._platform = (reader.ByteOrder == ByteOrder.Big) ?
                Platform.X360 : Platform.Xbox;

            Mounted = false;
        }

        public string Name
        {
            get { return _partitionName; }
        }

        public uint RootDirFirstCluster
        {
            get { return _rootDirFirstCluster; }
        }

        public long Length
        {
            get { return _partitionLength; }
        }

        public long FileAreaLength
        {
            get { return _fileAreaLength; }
        }

        public uint MaxClusters
        {
            get { return _maxClusters; }
        }

        public uint BytesPerCluster
        {
            get { return _bytesPerCluster; }
        }

        public DriveReader GetReader()
        { return _reader; }

        public DriveWriter GetWriter()
        { return _writer; }

        public uint[] FileAllocationTable
        {
            get { return _fileAllocationTable; }
        }

        public long FileAreaByteOffset
        {
            get { return _fileAreaByteOffset; }
        }

        public long Offset
        {
            get { return _partitionOffset; }
        }

        public List<DirectoryEntry> GetRoot()
        {
            return _root;
        }

        public Platform Platform
        {
            get { return _platform; }
        }

        public bool Mounted { get; private set; }

        /// <summary>
        /// Loads the FATX file system.
        /// </summary>
        public void Mount()
        {
            Mounted = false;

            // Read and verify volume metadata.
            ReadBootSector();
            CalculateOffsets();
            ReadFileAllocationTable();

            _root = ReadDirectoryStream(_rootDirFirstCluster);
            PopulateDirentStream(_root, _rootDirFirstCluster);

            Mounted = true;
        }

        /// <summary>
        /// Read and verifies the FATX header.
        /// </summary>
        private void ReadBootSector()
        {
            _reader.Seek(_partitionOffset); // Seek to the start of the partition. 
            //Console.WriteLine("Attempting to load FATX volume at {0:X16}", _reader.Position);

            if (!_usesLegacyFormat)
            {
                ReadVolumeMetadata(); // Read the volume metadata. 
                //
            }
            else
            {
                ReadLegacyVolumeMetadata();
            }
        }

        private void ReadVolumeMetadata()
        {
            _signature = _reader.ReadUInt32(); // XTAF Partition Offset Pos 0x00Position = 0x00000002856a000c
            _serialNumber = _reader.ReadUInt32(); // XTAF Partition Offset Pos 0x04
            _sectorsPerCluster = _reader.ReadUInt32();// XTAF Partition Offset Pos 0x08
            _rootDirFirstCluster = _reader.ReadUInt32(); // XTAF Partition Offset Pos 0x0C

            if (_signature != VolumeSignature)
            {
                Console.WriteLine($"Invalid FATX Signature for {_partitionName}: {_signature:X8}");
            }
        }
        //		_partitionLength	0x0000003663c7e000	long
        //		_partitionLength	0x0000000010000000	long

        private void ReadLegacyVolumeMetadata()
        {
            // Read _BOOT_SECTOR
            _jump = _reader.ReadUInt32();                   // EB FE
            _signature = _reader.ReadUInt32();              // FATX
            _serialNumber = _reader.ReadUInt32();           // 05C29C00

            if (_signature != VolumeSignature)
            {
                throw new FormatException($"Invalid FATX Signature for {_partitionName}: {_signature:X8}");
            }

            // Read BIOS_PARAMETER_BLOCK
            // These fields are currently not being used by this reader. With some testing, we'll see if these fields
            // are truly necessary.
            _bytesPerSector = _reader.ReadUInt16();         // 0200
            _sectorsPerCluster = _reader.ReadByte();        // 20
            _reservedSectors = _reader.ReadUInt16();        // 08
            _sectorsPerTrack = _reader.ReadUInt16();        // 3F00
            _heads = _reader.ReadUInt16();                  // FF00
            _hiddenSectors = _reader.ReadUInt32();          // 00000400
            _largeSectors = _reader.ReadUInt32();           // 009896B0
            _largeSectorsPerFat = _reader.ReadUInt32();     // 00000990
            _rootDirFirstCluster = _reader.ReadUInt32();    // 00000001
        }

        /// <summary>
        /// Calculate offsets needed to perform work on this file system.
        /// </summary>
        private void CalculateOffsets()
        {
            if (_sectorsPerCluster == 0x00000000)
            {
                this._sectorsPerCluster = 0x20;
                this._bytesPerCluster = 0x4000;
                this._partitionLength = 0xF1A8F2000;
            }
            else
            {
                _bytesPerCluster = _sectorsPerCluster * Constants.SectorSize;
                _maxClusters = (uint)(_partitionLength / (long)_bytesPerCluster) + Constants.ReservedClusters;
            }
            uint bytesPerFat;
            if (_maxClusters < 0xfff0)
            {
                bytesPerFat = _maxClusters * 2;
                _isFat16 = true;
            }
            else
            {
                bytesPerFat = _maxClusters * 4;
                _isFat16 = false;
                Console.WriteLine($"CalculateOffsets-> bytesPerFat: {bytesPerFat:X}");
            }

            _bytesPerFat = (bytesPerFat + (Constants.PageSize - 1)) &
                ~(Constants.PageSize - 1);
            Console.WriteLine($"CalculateOffsets-> _bytesPerFat: {_bytesPerFat:X}");
            this._fatByteOffset = Constants.ReservedBytes;
            this._fileAreaByteOffset = this._fatByteOffset + this._bytesPerFat;
            this._fileAreaLength = this.Length - this.FileAreaByteOffset;
            var Tempma = this._bytesPerFat + this._fatByteOffset + this._partitionOffset;
            Console.WriteLine($"CalculateOffsets-> _bytesPerCluster: {_bytesPerCluster:X} _maxClusters: {_maxClusters:X} _bytesPerFat: {_bytesPerFat:X} _fatByteOffset: {_fatByteOffset:X} _fileAreaByteOffset: {_fileAreaByteOffset:X} _fileAreaLength: {_fileAreaLength:X}");
            Console.WriteLine($"CalculateOffsets-> Tempma: {Tempma:X}");
        }

        /// <summary>
        /// Read either the 16 or 32 bit file allocation table.
        /// </summary>
        private void ReadFileAllocationTable()
        {
            _fileAllocationTable = new uint[_maxClusters];

            var fatOffset = ByteOffsetToPhysicalOffset(this._fatByteOffset);
            _reader.Seek(fatOffset);
            //Console.WriteLine($"ReadFileAllocationTable-> fatOffset: {fatOffset:X} _maxClusters: {_maxClusters:X} _isFat16: {_isFat16}");
            if (this._isFat16)
            {
                byte[] _tempFat = new byte[_maxClusters * 2];
                _reader.Read(_tempFat, (int)(_maxClusters * 2));

                if (_reader.ByteOrder == ByteOrder.Big)
                {
                    for (int i = 0; i < _maxClusters; i++)
                    {
                        Array.Reverse(_tempFat, i * 2, 2);
                    }
                }

                for (int i = 0; i < _maxClusters; i++)
                {
                    _fileAllocationTable[i] = BitConverter.ToUInt16(_tempFat, i * 2);
                }
            }
            else
            {
                byte[] _tempFat = new byte[_maxClusters * 4];
                _reader.Read(_tempFat, (int)(_maxClusters * 4));
                //Console.WriteLine($"_tempFat {_tempFat} {(_maxClusters * 4):X} ");
                if (_reader.ByteOrder == ByteOrder.Big)
                {
                    for (int i = 0; i < _maxClusters; i++)
                    {
                        Array.Reverse(_tempFat, i * 4, 4);
                    }
                }

                for (int i = 0; i < _maxClusters; i++)
                {
                    _fileAllocationTable[i] = BitConverter.ToUInt32(_tempFat, i * 4);
                }
            }
        }
        private const string VALID_CHARS = "abcdefghijklmnopqrstuvwxyz" +
                                   "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                   "0123456789" +
                                   "!#$%&\'()-.@[]^_`{}~ " +
                                   "\xff";
        private bool IsValidFileNameBytes(byte[] bytes)
        {
            foreach (byte b in bytes)
            {
                if (VALID_CHARS.IndexOf((char)b) == -1)
                {
                    return false;
                }
            }

            return true;
        }
        private bool IsValidFileNameLength(uint fileNameLength)
        {
            if (fileNameLength == 0x00 || fileNameLength == 0x01 || fileNameLength == 0xff)
            {
                return false;
            }

            if (fileNameLength > 0x2a && fileNameLength != 0xe5)
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Read a single directory stream.
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        private List<DirectoryEntry> ReadDirectoryStream(uint cluster)
        {
            List<DirectoryEntry> stream = new List<DirectoryEntry>();

            byte[] data = ReadCluster(cluster);
            long clusterOffset = ClusterToPhysicalOffset(cluster);
            //Console.WriteLine($"ReadDirectoryStream-> cluster: {cluster} clusterOffset: {clusterOffset:X}");

            for (int i = 0; i < 256; i++)
            {
                DirectoryEntry dirent = new DirectoryEntry(this.Platform, data, (i * 0x40));
                
                if (dirent.FileNameLength == Constants.DirentNeverUsed ||
                    dirent.FileNameLength == Constants.DirentNeverUsed2)
                {
                    // We are at the last dirent.
                    break;
                }
                if (!IsValidFileNameLength(dirent.FileNameLength))
                {
                    continue;
                }
                if (!IsValidFileNameBytes(dirent.FileNameBytes))
                {
                    continue;
                }
                //Console.WriteLine($"ReadDirectoryStream-> Offset: {dirent.Offset:X}");
                dirent.Offset = clusterOffset + (i * 0x40);
                //onsole.WriteLine($"ReadDirectoryStream-> dirent.FileName: {dirent.FileName} Offset: {dirent.Offset:X}");
                stream.Add(dirent);

            }

            return stream;
        }

        /// <summary>
        /// Iterates dirent's from stream and populates directories with its child
        /// dirents. 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="clusterIndex"></param>
        //private void PopulateDirentStream(List<DirectoryEntry> stream, uint clusterIndex)
        //{
        //    foreach (DirectoryEntry dirent in stream)
        //    {
        //        dirent.Cluster = clusterIndex;
        //      //  Console.WriteLine(String.Format("{0}", dirent.FileName));

        //        if (dirent.IsDirectory() && !dirent.IsDeleted())
        //        {
        //            //Console.WriteLine($"PopulateDirentStream-> dirent.FileName: {dirent.FileName} is a directory. FirstCluster: {dirent.FirstCluster}");
        //            List<uint> chainMap = GetClusterChain(dirent);

        //            foreach (uint cluster in chainMap)
        //            {
        //              //  Console.WriteLine($"PopulateDirentStream-> foreach uint cluster in chainMap {chainMap} cluster: {cluster}");
        //                List<DirectoryEntry> direntStream = ReadDirectoryStream(cluster);
        //                // To print the contents of direntStream (which is a List<DirectoryEntry>), you should iterate and print each entry's details.
        //                // Example usage inside PopulateDirentStream or similar context:
        //                foreach (var entry in direntStream)
        //                {
        //                    //Console.WriteLine($"Entry: FileName {entry.FileName}, Offset {entry.Offset:X}, IsDirectory {entry.IsDirectory()}, IsDeleted {entry.IsDeleted()}, FirstCluster {entry.FirstCluster}, FileSize {entry.FileSize}");
        //                }
        //                dirent.AddChildren(direntStream);

        //                PopulateDirentStream(direntStream, cluster);
        //            }
        //        }
        //    }
        //}
        private void PopulateDirentStream(List<DirectoryEntry> stream, uint clusterIndex)
        {
            foreach (DirectoryEntry dirent in stream)
            {
                dirent.Cluster = clusterIndex;
                Console.WriteLine(String.Format("{0}", dirent.FileName));

                if (dirent.IsDirectory() && !dirent.IsDeleted())
                {
                    List<uint> chainMap = GetClusterChain(dirent);

                    foreach (uint cluster in chainMap)
                    {
                        List<DirectoryEntry> direntStream = ReadDirectoryStream(cluster);

                        dirent.AddChildren(direntStream);

                        PopulateDirentStream(direntStream, cluster);
                    }
                }
            }
        }
        /// <summary>
        /// Seek to any offset in the file area.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        public void SeekFileArea(long offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            // TODO: Check for invalid offset
            offset += FileAreaByteOffset + _partitionOffset;
            //Console.WriteLine($"SeekFileArea-> After += offset: {offset}");
            _reader.Seek(offset, origin);
        }

        /// <summary>
        /// Seek to a cluster.
        /// </summary>
        /// <param name="cluster"></param>
        public void SeekToCluster(uint cluster)
        {
            var offset = ClusterToPhysicalOffset(cluster);
            //Console.WriteLine($"SeekToCluster-> offset: {offset}");
            _reader.Seek(offset);
        }

        /// <summary>
        /// Reads a cluster and returns the data.
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        public byte[] ReadCluster(uint cluster)
        {
            var clusterOffset = ClusterToPhysicalOffset(cluster);
            _reader.Seek(clusterOffset);
            byte[] clusterData = new byte[_bytesPerCluster];
            _reader.Read(clusterData, (int)_bytesPerCluster);
            //Console.WriteLine($"ReadCluster-> clusterOffset: {clusterOffset} ReadCluster->clusterData: {clusterData}");
            return clusterData;
        }

        /// <summary>
        /// Get a cluster chain for a dirent from the file allocation table.
        /// </summary>
        /// <param name="dirent">The DirectoryEntry to get the cluster chain for.</param>
        /// <returns></returns>
        public List<uint> GetClusterChain(DirectoryEntry dirent)
        {
            var firstCluster = dirent.FirstCluster;
            List<uint> clusterChain = new List<uint>();

            if (firstCluster == 0 || firstCluster > this.MaxClusters)
            {
                //Console.WriteLine($" First cluster invalid (FirstCluster= {firstCluster} MaxClusters= {this.MaxClusters})");
                return clusterChain;
            }

            clusterChain.Add(firstCluster);

            if (dirent.IsDeleted())
            {
                return clusterChain;
            }

            uint fatEntry = firstCluster;
            uint reservedIndexes = (_isFat16) ? Constants.Cluster16Reserved : Constants.ClusterReserved;
            while (true)
            {
                fatEntry = _fileAllocationTable[fatEntry];
                if (fatEntry >= reservedIndexes)
                {
                    break;
                }

                if (fatEntry == 0 || fatEntry > _fileAllocationTable.Length)
                {
                    // TODO: Warn user.
                  //  Console.WriteLine($"File {dirent.FileName} has a corrupt cluster chain");
                    clusterChain = new List<uint>(1);
                    clusterChain.Add(firstCluster);
                    return clusterChain;
                }

                // Get next cluster.
                clusterChain.Add(fatEntry);
            }

            return clusterChain;
        }

        /// <summary>
        /// Convert a byte offset (relative to the volume) to an offset into the
        /// image file it belongs to.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        private long ByteOffsetToPhysicalOffset(long offset)
        {
            return this._partitionOffset + offset;
        }

        /// <summary>
        /// Convert a cluster index to an offset into the image file it belongs to.
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        public long ClusterToPhysicalOffset(uint cluster)
        {
            var physicalOffset = ByteOffsetToPhysicalOffset(FileAreaByteOffset);
            long clusterOffset = (long)_bytesPerCluster * (long)(cluster - 1);
            return (physicalOffset + clusterOffset);
        }

        private long CountFiles(List<DirectoryEntry> dirents)
        {
            long numFiles = 0;

            foreach (var dirent in dirents)
            {
                numFiles += dirent.CountFiles();
            }

            return numFiles;
        }

        public long CountFiles()
        {
            return CountFiles(_root);
        }

        public long GetTotalSpace()
        {
            return _fileAreaLength;
        }

        public long GetFreeSpace()
        {
            return (_fileAreaLength) - GetUsedSpace();
        }

        public long GetUsedSpace()
        {
            // Count number of used clusters
            long clustersUsed = 0;

            foreach (var cluster in FileAllocationTable)
            {
                if (cluster != Constants.ClusterAvailable)
                {
                    clustersUsed++;
                }
            }

            return (clustersUsed * BytesPerCluster);
        }
    }
}
