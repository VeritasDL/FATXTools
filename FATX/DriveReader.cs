using FATX.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Diagnostics;

namespace FATX
{
    public class DriveReader : EndianReader
    {

        private List<Volume> _partitions = new List<Volume>();
        public DriveReader(Stream stream)
            : base(stream)
        {
        }

        public void Initialize()
        {
            Seek(0);

            // Definitely need to refactor all of this..
            // Check for memory unit image
            if (ReadUInt64() == 0x534F44534D9058EB)
            {
                Console.WriteLine("Mounting Xbox 360 Memory Unit..");

                ByteOrder = ByteOrder.Big;
                AddPartition("Storage", 0x20E2A000, 0xCE1D0000);
                AddPartition("SystemExtPartition", 0x13FFA000, 0xCE30000);
                AddPartition("SystemURLCachePartition", 0xDFFA000, 0x6000000);
                AddPartition("TitleURLCachePartition", 0xBFFA000, 0x2000000);
                AddPartition("StorageSystem", 0x7FFA000, 0x4000000);
                return;
            }

            // Might move to a new class
            // Check for Original XBOX partition.
            Seek(0xABE80000);
            if (ReadUInt32() == 0x58544146)
            {
                Console.WriteLine("Mounting Xbox Original HDD..");

                AddPartition("Partition1", 0xABE80000, 0x1312D6000);    // DATA
                AddPartition("Partition2", 0x8CA80000, 0x1f400000);     // SHELL
                AddPartition("Partition3", 0x5DC80000, 0x2ee00000);     // CACHE
                AddPartition("Partition4", 0x2EE80000, 0x2ee00000);     // CACHE
                AddPartition("Partition5", 0x80000, 0x2ee00000);        // CACHE
                return;
            }

            // Check for Original XBOX DVT3 (Prototype Development Kit) partition.
            Seek(0x80000);
            if (ReadUInt32() == 0x58544146)
            {
                Console.WriteLine("Mounting Xbox DVT3 HDD (v2)..");

                AddPartition("Partition1", 0x80000, 0x1312D6000);        // DATA
                AddPartition("Partition2", 0x131356000, 0x1f400000);     // SHELL
                AddPartition("Partition3", 0x150756000, 0x2ee00000);     // CACHE
                AddPartition("Partition4", 0x17F556000, 0x2ee00000);     // CACHE
                AddPartition("Partition5", 0x1AE356000, 0x2ee00000);     // CACHE
                return;
            }

            Seek(0x80004);
            if (ReadUInt32() == 0x58544146)
            {
                Console.WriteLine("Mounting Xbox DVT3 HDD (v1)..");

                AddPartition("Partition1", 0x80000, 0x1312D6000, true);        // DATA
                AddPartition("Partition2", 0x131356000, 0x1f400000, true);     // SHELL
                AddPartition("Partition3", 0x150756000, 0x2ee00000, true);     // CACHE
                AddPartition("Partition4", 0x17F556000, 0x2ee00000, true);     // CACHE
                AddPartition("Partition5", 0x1AE356000, 0x2ee00000, true);     // CACHE
                return;
            }

            // Check for XBOX 360 partitions.
            Seek(0);
            ByteOrder = ByteOrder.Big;
            if (ReadUInt32() == 0x20000)
            {
                Console.WriteLine("Mounting Xbox 360 Dev HDD..");

                // This is a dev formatted HDD.
                ReadUInt16();  // Kernel version
                ReadUInt16();
                // TODO: reading from raw devices requires sector aligned reads.
                Seek(8);
                // Partition1
                long dataOffsetVal = (long)ReadUInt32();
                long dataLengthVal = (long)ReadUInt32();
                Seek(0);
                Seek(8);
                long dataOffset = (long)ReadUInt32() * Constants.SectorSize;
                long dataLength = (long)ReadUInt32() * Constants.SectorSize;

                // SystemPartition
                long shellOffset = (long)ReadUInt32() * Constants.SectorSize;
                long shellLength = (long)ReadUInt32() * Constants.SectorSize;
                // Unused?
                ReadUInt32();
                ReadUInt32();
                // DumpPartition
                ReadUInt32();
                ReadUInt32();
                // PixDump
                ReadUInt32();
                ReadUInt32();
                // Unused?
                ReadUInt32();
                ReadUInt32();
                // Unused?
                ReadUInt32();
                ReadUInt32();
                // AltFlash
                ReadUInt32();
                ReadUInt32();
                // Cache0
                long cache0Offset = (long)ReadUInt32() * Constants.SectorSize;
                long cache0Length = (long)ReadUInt32() * Constants.SectorSize;
                // Cache1
                long cache1Offset = (long)ReadUInt32() * Constants.SectorSize;
                long cache1Length = (long)ReadUInt32() * Constants.SectorSize;
                
                AddPartition("Partition1", dataOffset, dataLength);
                AddPartition("SystemPartition", shellOffset, shellLength);
                // TODO: Add support for these
                //AddPartition("Cache0", cache0Offset, cache0Length);
                //AddPartition("Cache1", cache1Offset, cache1Length);
            }
            else
            {
                Console.WriteLine("Mounting Xbox 360 Retail HDD..");

                //Seek(8);
                //var test = ReadUInt32();

                // This is a retail formatted HDD.
                /// Partition0 0, END
                /// Cache0 0x80000, 0x80000000
                /// Cache1 0x80080000, 0x80000000
                /// DumpPartition 0x100080000, 0x20E30000
                ///   SystemURLCachePartition 0, 0x6000000
                ///   TitleURLCachePartition 0x6000000, 0x2000000
                ///   SystemExtPartition 0x0C000000, 0x0CE30000
                ///   SystemAuxPartition 0x18e30000, 0x08000000
                /// SystemPartition 0x120EB0000, 0x10000000
                /// Partition1 0x130EB0000, END

                AddPartition("Partition1", 0x130eb0000, this.Length - 0x130eb0000);
                AddPartition("SystemPartition", 0x120eb0000, 0x10000000);

                // 0x118EB0000 - 0x100080000
                // TODO: Add support for these
                //AddPartition("Cache0", 0x80000, 0x80000000);
                //AddPartition("Cache1", 0x80080000, 0x80000000);

                const long dumpPartitionOffset = 0x100080000;
                // TODO: Add support for these
                //AddPartition("DumpPartition", 0x100080000, 0x20E30000);
                //AddPartition("SystemURLCachePartition", dumpPartitionOffset + 0, 0x6000000);
                //AddPartition("TitleURLCachePartition", dumpPartitionOffset + 0x6000000, 0x2000000);
                //AddPartition("SystemExtPartition", dumpPartitionOffset + 0x0C000000, 0xCE30000);
                AddPartition("SystemAuxPartition", dumpPartitionOffset + 0x18e30000, 0x8000000);
            }
        }

        public void AddPartition(string name, long offset, long length, bool legacy = false)
        {
            Volume partition = new Volume(this, name, offset, length, legacy);
            _partitions.Add(partition);
        }
        public List<(string Name, long Offset, long Length)> GetDevkitHeaderPartitions()
        {
            const uint SectorSize = Constants.SectorSize;
            ByteOrder = ByteOrder.Big;
            var partitionArray = new List<KeyValuePair<long, string>>();
            var entries1 = new List<(int Index, long Offset, long Length)>();
            var partitionarray = new List<KeyValuePair<ulong, string>>
            {
                new KeyValuePair<ulong, string>(0x28C080000, "SystemExtPartition"),
                new KeyValuePair<ulong, string>(0x298EB0000, "SystemAuxPartition"),
                new KeyValuePair<ulong, string>(0x2C0EB0000, "BackCompatPartition"),
                new KeyValuePair<ulong, string>(0x2D0EB0000, "ContentPartition"),
                new KeyValuePair<ulong, string>(0x3934B2E000, "AltFlash")
            };
            var knownOffsets = partitionarray.ToDictionary(p => (long)p.Key, p => p.Value);
            var entries = new List<(int Index, long Offset, long Length)>();

            // Parse header entries
            Seek(8); // Skip kernel version
            for (int i = 0; i < 10; i++)
            {
                uint offsetSectors = ReadUInt32();
                uint lengthSectors = ReadUInt32();

                long offset = (long)offsetSectors * SectorSize;
                long length = (long)lengthSectors * SectorSize;

                if (offset != 0)
                {
                    entries.Add((i, offset, length));
                    entries1.Add((i, offset, length));
                    Console.WriteLine($"Header entry: 0x{i:X4}, Offset: 0x{offset:X}, Length: 0x{length:X}");
                }
            }

            entries.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            entries1.Sort((a, b) => a.Offset.CompareTo(b.Offset));
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Length == 0)
                {
                    long nextOffset = (i + 1 < entries.Count) ? entries[i + 1].Offset : 0;
                    entries[i] = (entries[i].Index, entries[i].Offset, nextOffset - entries[i].Offset);
                }
            }

            var result = new List<(string Name, long Offset, long Length)>();
            var seenOffsets = new HashSet<long>();

            foreach (var (index, offset, length) in entries)
            {
                seenOffsets.Add(offset);
                if (knownOffsets.TryGetValue(offset, out var knownName))
                {
                    result.Add((knownName, offset, length));
                }
                else
                {
                    Seek(offset);
                    if (ReadUInt32() == 0x20000)
                    {
                        uint serial = ReadUInt32();
                        uint spc = ReadUInt32();
                        uint rootCluster = ReadUInt32();
                        ushort volumeNameValue = ReadUInt16();
                        string volumeName = $"0x{volumeNameValue:X4}";
                        result.Add((volumeName, offset, length));
                    }
                    else
                    {
                        result.Add(($"Unknown{index}", offset, length));
                    }
                }
            }

            // Post-header known offsets
            var remainingPartitions = partitionarray
                .Select(p => (Offset: (long)p.Key, Name: p.Value))
                .Where(p => !seenOffsets.Contains(p.Offset))
                .OrderBy(p => p.Offset)
                .ToList();

            for (int i = 0; i < remainingPartitions.Count; i++)
            {
                long offset = remainingPartitions[i].Offset;
                Console.WriteLine($"offset: 0x{offset:X4} (post-header)");
                string label = remainingPartitions[i].Name;
                long length = (i + 1 < remainingPartitions.Count)? remainingPartitions[i + 1].Offset - remainingPartitions[i].Offset: 0x3A38B2DFFF - offset;
                Console.WriteLine($"length: 0x{length:X4} (post-header)");
                ByteOrder = ByteOrder.Big;
                Seek(offset);
                if (ReadUInt32() == 0x20000)
                {
                    uint serial = ReadUInt32();
                    uint spc = ReadUInt32();
                    uint rootCluster = ReadUInt32();
                    ushort volumeNameValue = ReadUInt16();
                    string volumeName = $"0x{volumeNameValue:X4}";
                    result.Add((volumeName, offset, length));
                }
                else
                {
                    result.Add((remainingPartitions[i].Name, offset, length));
                }

                seenOffsets.Add(offset);
            }
            var partitionArray2 = result.OrderBy(p => p.Offset).ToList(); // Ensure partitions are sorted by offset
            var xtafArray = new List<long>();
            // Brute-force XTAF partitions
            const uint magicXTAF = 0x58544146;
            for (long offset = 0; offset < this.Length - 0x3A38B2DFFF; offset += 0x200)
            {
                if (seenOffsets.Contains(offset))
                    continue;
                Seek(offset);
                uint magic = ReadUInt32();
                if (magic == magicXTAF)
                {
                    Console.WriteLine($"XTAF Partition Magic Found at 0x{offset:X4}");
                    Debug.WriteLine($"XTAF Partition Magic Found at 0x{offset:X4}");
                    xtafArray.Add(offset);
                    Seek(offset + 8);
                    uint spc = ReadUInt32();
                    Console.WriteLine($"SectorsPerCluster at 0x{offset + 8:X}: 0x{spc:X}");
                    Debug.WriteLine($"SectorsPerCluster at 0x{offset + 8:X}: 0x{spc:X}");
                    if (spc == 0 || spc > 128 || (spc & (spc - 1)) != 0)
                    {
                        Console.WriteLine($"Invalid SectorsPerCluster at 0x{offset:X}: 0x{spc:X}");
                        Debug.WriteLine($"Invalid SectorsPerCluster at 0x{offset:X}: 0x{spc:X}");
                        continue;
                    }
                    long clusterSize = spc * SectorSize;
                    Console.WriteLine($"ClusterSize: 0x{clusterSize:X}");
                    Debug.WriteLine($"ClusterSize: 0x{clusterSize:X}");
                    long fatStart = offset + 0x1000;
                    Console.WriteLine($"FAT Start Offset: 0x{fatStart:X}");
                    Debug.WriteLine($"FAT Start Offset: 0x{fatStart:X}");
                    long scanOffset = fatStart;
                    long firstClusterOffset = -1;

                    while (scanOffset < this.Length - 4)
                    {
                        Seek(scanOffset);
                        uint entry = ReadUInt32();
                        if (entry != 0)
                        {
                            firstClusterOffset = scanOffset;
                            Console.WriteLine($"First non-zero FAT entry at 0x{firstClusterOffset:X}, value: 0x{entry:X}");
                            Debug.WriteLine($"First non-zero FAT entry at 0x{firstClusterOffset:X}, value: 0x{entry:X}");
                            break;
                        }
                        scanOffset += 4;
                        Console.WriteLine($"scanOffset 0x{scanOffset:X}");
                        Debug.WriteLine($"scanOffset 0x{scanOffset:X}");
                    }

                    if (firstClusterOffset == -1)
                    {
                        Console.WriteLine($"No non-zero FAT entry found after 0x{fatStart:X}");
                        Debug.WriteLine($"No non-zero FAT entry found after 0x{fatStart:X}");
                        continue;
                    }

                    long fatLength = firstClusterOffset - fatStart;
                    Console.WriteLine($"FAT Length: 0x{fatLength:X}");
                    Debug.WriteLine($"FAT Length: 0x{fatLength:X}");
                    long clusterCount = fatLength / 4;
                    Console.WriteLine($"Cluster Count: {clusterCount}");
                    Debug.WriteLine($"Cluster Count: {clusterCount}");
                    long partitionLength = clusterCount * clusterSize;
                    Console.WriteLine($"Estimated Partition Length: 0x{partitionLength:X}");
                    Debug.WriteLine($"Estimated Partition Length: 0x{partitionLength:X}");
                    result.Add(($"XTAF@0x{offset:X}", offset, partitionLength));
                    seenOffsets.Add(offset);
                }
            }

            // Deduplicate by offset and sort
            result = result
                .GroupBy(p => p.Offset)
                .Select(g => g.First())
                .OrderBy(p => p.Offset)
                .ToList();

            return result;
        }
        public Volume GetPartition(int index)
        {
            return _partitions[index];
        }

        public List<Volume> Partitions => _partitions;
    }
}
