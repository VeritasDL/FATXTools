using FATX;
using FATX.Analyzers;
using FATX.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Windows.AI.MachineLearning;

namespace FATXTools.Database
{
    public class PartitionDatabase
    {
        Volume volume;
        FileCarver fileCarver;  // TODO: Get rid of this. We should be able to get this information from the FileDatabase.
        bool metadataAnalyzer;
        FileDatabase fileDatabase;
        PartitionView view; // TODO: Use events instead.
        private HashSet<long> usedOffsets = new HashSet<long>();
        private Dictionary<uint, int> direntsWrittenPerCluster = new Dictionary<uint, int>();
        private readonly HashSet<string> usedPaths = new(StringComparer.OrdinalIgnoreCase);
        public event EventHandler OnLoadRecoveryFromDatabase;
        private int _currentYear = DateTime.Now.Year;
        private const string VALID_CHARS = "abcdefghijklmnopqrstuvwxyz" +
                                           "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                                           "0123456789" +
                                           "!#$%&\'()-.@[]^_`{}~ " +
                                           "\xff";
        private const int DirentSize = 0x40;
        public PartitionDatabase(Volume volume)
        {
            this.volume = volume;
            this.metadataAnalyzer = false;
            this.fileCarver = null;
            this.fileDatabase = new FileDatabase(volume);
        }
        private DirectoryEntry dirent;
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

        public const int ValidAttributes = 55;

        /// <summary>
        /// Validate FileAttributes.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public bool IsValidAttributes(FileAttribute attributes)
        {
            if (attributes == 0)
            {
                return true;
            }

            if (!Enum.IsDefined(typeof(FileAttribute), attributes))
            {
                return false;
            }

            return true;
        }

        private int[] MaxDays =
        {
              31, // Jan
              29, // Feb
              31, // Mar
              30, // Apr
              31, // May
              30, // Jun
              31, // Jul
              31, // Aug
              30, // Sep
              31, // Oct
              30, // Nov
              31  // Dec
        };

        /// <summary>
        /// Validate a TimeStamp.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        private bool IsValidDateTime(TimeStamp dateTime)
        {
            if (dateTime == null)
            {
                return false;
            }

            // TODO: create settings to customize these specifics
            if (dateTime.Year > _currentYear)
            {
                return false;
            }

            if (dateTime.Year > 2038 || dateTime.Year < 1981)
            {
                return false;
            }

            if (dateTime.Month > 12 || dateTime.Month < 1)
            {
                return false;
            }

            if (dateTime.Day > MaxDays[dateTime.Month - 1] || dateTime.Day < 1)
            {
                return false;
            }

            if (dateTime.Hour > 23 || dateTime.Hour < 0)
            {
                return false;
            }

            if (dateTime.Minute > 59 || dateTime.Minute < 0)
            {
                return false;
            }

            if (dateTime.Second > 59 || dateTime.Second < 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate FirstCluster.
        /// </summary>
        /// <param name="firstCluster"></param>
        /// <returns></returns>
        private bool IsValidFirstCluster(uint firstCluster)
        {
            if (firstCluster > this.volume.MaxClusters)
            {
                return false;
            }

            // NOTE: deleted files have been found with firstCluster set to 0
            //  To be as thorough as we can, let's include those.
            // (reverted to work around an issue)
            if (firstCluster == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validate FileNameLength.
        /// </summary>
        /// <param name="fileNameLength"></param>
        /// <returns></returns>
        private bool IsValidFileNameLength(uint fileNameLength)
        {
            if (fileNameLength == 0x00 || fileNameLength == 0x01 || fileNameLength == 0xff)
            {
                return false;
            }

            if (fileNameLength > 42 && fileNameLength != 0xe5)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if dirent is actually a dirent.
        /// </summary>
        /// <param name="dirent"></param>
        /// <returns></returns>
        public bool IsValidDirent(DirectoryEntry dirent)
        {
            if (!IsValidFileNameLength(dirent.FileNameLength))
            {
                return false;
            }

            if (!IsValidFirstCluster(dirent.FirstCluster))
            {
                return false;
            }

            if (!IsValidFileNameBytes(dirent.FileNameBytes))
            {
                return false;
            }

            if (!IsValidAttributes(dirent.FileAttributes))
            {
                return false;
            }

            if (!IsValidDateTime(dirent.CreationTime) ||
                !IsValidDateTime(dirent.LastAccessTime) ||
                !IsValidDateTime(dirent.LastWriteTime))
            {
                return false;
            }

            return true;
        }

        private bool IsDirentDeleted(DirectoryEntry dirent)
        {
            // Check if the first byte of the file name is 0xE5
            if (dirent.FileNameLength == 0xE5)
            {
                return true;
            }
            return false;
        }

        public string PartitionName => volume.Name;
        public Volume Volume => volume;

        public void SetPartitionView(PartitionView view) => this.view = view;

        public void SetMetadataAnalyzer(bool metadataAnalyzer) => this.metadataAnalyzer = metadataAnalyzer;

        public void SetFileCarver(FileCarver fileCarver) => this.fileCarver = fileCarver;

        public FileDatabase GetFileDatabase() => this.fileDatabase;

        public void Save(Dictionary<string, object> partitionObject)
        {
            partitionObject["Name"] = this.volume.Name;
            partitionObject["Offset"] = this.volume.Offset;
            partitionObject["Length"] = this.volume.Length;

            partitionObject["Analysis"] = new Dictionary<string, object>();
            var analysisObject = partitionObject["Analysis"] as Dictionary<string, object>;

            analysisObject["MetadataAnalyzer"] = new List<Dictionary<string, object>>();
            if (metadataAnalyzer)
            {
                var metadataAnalysisList = analysisObject["MetadataAnalyzer"] as List<Dictionary<string, object>>;
                SaveMetadataAnalysis(metadataAnalysisList);
            }

            analysisObject["FileCarver"] = new List<Dictionary<string, object>>();
            if (fileCarver != null)
            {
                var fileCarverObject = analysisObject["FileCarver"] as List<Dictionary<string, object>>;
                SaveFileCarver(fileCarverObject);
            }
        }
        private void SaveMetadataAnalysis(List<Dictionary<string, object>> metadataAnalysisList)
        {
            foreach (var databaseFile in fileDatabase.GetRootFiles())
            {
                var directoryEntryObject = new Dictionary<string, object>();
                metadataAnalysisList.Add(directoryEntryObject);
                SaveDirectoryEntry(directoryEntryObject, databaseFile);
            }
        }
        private void PadDirentCluster(uint cluster)
        {
            if (!direntsWrittenPerCluster.TryGetValue(cluster, out int count)) return;

            long clusterOffset = this.volume.ClusterToPhysicalOffset(cluster);
            int bytesPerCluster = (int)this.volume.BytesPerCluster;
            int writtenBytes = count * DirentSize;
            long padStart = clusterOffset + writtenBytes;
            int padLen = bytesPerCluster - writtenBytes;

            while (padLen > 0)
            {
                this.volume.GetReader().Seek(padStart);
                byte[] dataCheck = new byte[0x40];
                this.volume.GetReader().Read(dataCheck, DirentSize);
                var direntCheck = new DirectoryEntry(this.volume.Platform, dataCheck, 0);

                if (!IsValidDirent(direntCheck))
                {
                    this.volume.GetReader().Seek(padStart);
                    int thisPad = Math.Min(padLen, DirentSize);
                    byte[] pad = Enumerable.Repeat((byte)0xFF, thisPad).ToArray();
                    this.volume.GetReader().Write(pad, thisPad);
                }

                padStart += DirentSize;
                padLen -= DirentSize;
            }
        }

        public void FinalizeDirentStreams()
        {
            foreach (var kvp in direntsWrittenPerCluster)
            {
                PadDirentCluster(kvp.Key);
            }
            Console.WriteLine($"Padded" + direntsWrittenPerCluster.Sum(kvp => kvp.Value) + " dirents");
        }

        private void SaveFileCarver(List<Dictionary<string, object>> fileCarverList)
        {
            foreach (var file in fileCarver.GetCarvedFiles())
            {
                var fileCarverObject = new Dictionary<string, object>();
                fileCarverObject["Offset"] = file.Offset;
                fileCarverObject["Name"] = file.FileName;
                fileCarverObject["Size"] = file.FileSize;
                fileCarverList.Add(fileCarverObject);
            }
        }
        // Helper: finds file in NonDeleted roots by name and size
        private string FindFileInRoots(List<string> roots, string fileName, long size)
        {
            foreach (var root in roots)
            {
                if (Directory.Exists(root))
                {
                    foreach (var f in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                    {
                        var fi = new FileInfo(f);
                        if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == size)
                            return f;
                    }
                }
                else if (File.Exists(root))
                {
                    var fi = new FileInfo(root);
                    if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == size)
                        return root;
                }
            }
            return null;
        }

        // Helper: finds file in Recovered roots, inside Cluster N folders, by name/size
        private string FindFileInClusteredRoots(List<string> roots, JsonElement entry, string fileName, long size)
        {

            uint cluster = entry.TryGetProperty("Cluster", out var clElem) ? clElem.GetUInt32() : 0;
            foreach (var root in roots)
            {
                string clusterFolder = Path.Combine(root, $"Cluster {cluster}");
                if (Directory.Exists(clusterFolder))
                {
                    foreach (var f in Directory.EnumerateFiles(clusterFolder, "*", SearchOption.AllDirectories))
                    {
                        var fi = new FileInfo(f);
                        if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == size)
                            return f;
                    }
                }
            }
            return null;
        }
        private void SaveDirectoryEntry(Dictionary<string, object> directoryEntryObject, DatabaseFile directoryEntry)
        {
            directoryEntryObject["Cluster"] = directoryEntry.Cluster;
            directoryEntryObject["Offset"] = directoryEntry.Offset;

            directoryEntryObject["FileNameLength"] = directoryEntry.FileNameLength;
            directoryEntryObject["FileAttributes"] = (byte)directoryEntry.FileAttributes;
            directoryEntryObject["FileName"] = directoryEntry.FileName;
            directoryEntryObject["FileNameBytes"] = directoryEntry.FileNameBytes;
            directoryEntryObject["FirstCluster"] = directoryEntry.FirstCluster;
            directoryEntryObject["FileSize"] = directoryEntry.FileSize;
            directoryEntryObject["CreationTime"] = directoryEntry.CreationTime.AsInteger();
            directoryEntryObject["LastWriteTime"] = directoryEntry.LastWriteTime.AsInteger();
            directoryEntryObject["LastAccessTime"] = directoryEntry.LastAccessTime.AsInteger();

            if (directoryEntry.IsDirectory())
            {
                directoryEntryObject["Children"] = new List<Dictionary<string, object>>();
                var childrenList = directoryEntryObject["Children"] as List<Dictionary<string, object>>;
                foreach (var child in directoryEntry.Children)
                {
                    var childObject = new Dictionary<string, object>();
                    childrenList.Add(childObject);
                    SaveDirectoryEntry(childObject, child);
                }
            }
            directoryEntryObject["Clusters"] = directoryEntry.ClusterChain;
        }

        // Updated: Add the new overload for recoveryJson/allRoots logic
        // Overload for recovery JSON: attempts to map to physical files in recovery folders and writes file data if found
        public DatabaseFile LoadDirectoryEntryFromDatabase(JsonElement directoryEntryObject, bool recoveryJson, List<string> allRoots)
        {
            JsonElement offsetElement;
            if (!directoryEntryObject.TryGetProperty("Offset", out offsetElement))
            {
                Console.WriteLine("Failed to load metadata object from database: Missing offset field");
                return null;
            }
            JsonElement clusterElement;
            if (!directoryEntryObject.TryGetProperty("Cluster", out clusterElement))
            {
                Console.WriteLine("Failed to load metadata object from database: Missing cluster field");
                return null;
            }
            long offset = offsetElement.GetInt64();
            uint cluster = clusterElement.GetUInt32();
            if (offset < this.volume.Offset || offset > this.volume.Offset + this.volume.Length)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid offset {offset}");
                return null;
            }
            if (cluster < 0 || cluster > this.volume.MaxClusters)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid cluster {cluster}");
                return null;
            }

            byte[] data = new byte[0x40];
            byte fileNameLength = directoryEntryObject.GetProperty("FileNameLength").GetByte();
            byte fileAttributes = directoryEntryObject.GetProperty("FileAttributes").GetByte();
            string fileName = directoryEntryObject.GetProperty("FileName").GetString();
            byte[] fileNameBytes = directoryEntryObject.GetProperty("FileNameBytes").GetBytesFromBase64();
            uint firstCluster = directoryEntryObject.GetProperty("FirstCluster").GetUInt32();
            uint fileSize = directoryEntryObject.GetProperty("FileSize").GetUInt32();
            uint creationTime = directoryEntryObject.GetProperty("CreationTime").GetUInt32();
            uint lastWriteTime = directoryEntryObject.GetProperty("LastWriteTime").GetUInt32();
            uint lastAccessTime = directoryEntryObject.GetProperty("LastAccessTime").GetUInt32();

            data[0x00] = fileNameLength;
            data[0x01] = fileAttributes;
            Buffer.BlockCopy(fileNameBytes, 0, data, 0x02, 0x2A);

            if (this.volume.Platform == Platform.Xbox)
            {
                Array.Copy(BitConverter.GetBytes(firstCluster), 0, data, 0x2C, 4);
                Array.Copy(BitConverter.GetBytes(fileSize), 0, data, 0x30, 4);
                Array.Copy(BitConverter.GetBytes(creationTime), 0, data, 0x34, 4);
                Array.Copy(BitConverter.GetBytes(lastWriteTime), 0, data, 0x38, 4);
                Array.Copy(BitConverter.GetBytes(lastAccessTime), 0, data, 0x3C, 4);
            }
            else if (this.volume.Platform == Platform.X360)
            {
                byte[] fc = BitConverter.GetBytes(firstCluster); Array.Reverse(fc); Array.Copy(fc, 0, data, 0x2C, 4);
                byte[] fs = BitConverter.GetBytes(fileSize); Array.Reverse(fs); Array.Copy(fs, 0, data, 0x30, 4);
                byte[] ct = BitConverter.GetBytes(creationTime); Array.Reverse(ct); Array.Copy(ct, 0, data, 0x34, 4);
                byte[] lwt = BitConverter.GetBytes(lastWriteTime); Array.Reverse(lwt); Array.Copy(lwt, 0, data, 0x38, 4);
                byte[] lat = BitConverter.GetBytes(lastAccessTime); Array.Reverse(lat); Array.Copy(lat, 0, data, 0x3C, 4);
            }

            if (recoveryJson)
            {
                // Write the dirent to its offset
                this.volume.GetReader().Seek(offset);
                this.volume.GetReader().Write(data, 0x40);
                DirectoryEntry direntma2 = new DirectoryEntry(this.volume.Platform, data, 0);
                string recoveredPath = null;
                bool isDirectory = (fileAttributes & 0x10) != 0;
                if (isDirectory)
                {
                    if (direntsWrittenPerCluster.ContainsKey(cluster))
                        direntsWrittenPerCluster[cluster]++;
                    else
                        direntsWrittenPerCluster[cluster] = 1;
                    // Write the dirent stream for all children to FirstCluster
                    List<JsonElement> childrenList = new List<JsonElement>();
                    if (directoryEntryObject.TryGetProperty("Children", out var childrenElement1))
                    {
                        foreach (var childElement in childrenElement1.EnumerateArray())
                            childrenList.Add(childElement);
                    }
                    if (childrenList.Count > 0)
                    {
                        long firstClusterOffset = this.volume.ClusterToPhysicalOffset(firstCluster);
                        this.volume.GetReader().Seek(firstClusterOffset);
                        foreach (var childElement in childrenList)
                        {
                            byte[] childData = BuildDirentFromJson(childElement);
                            this.volume.GetReader().Write(childData, DirentSize);
                        }
                        PadDirentCluster(firstCluster);
                    }
                }
                else // file Console.WriteLine($"{o.GetRootDirectoryEntry().Cluster}/{o.GetFullPath()}");
                {
                    DirectoryEntry direntma1 = new DirectoryEntry(this.volume.Platform, data, 0);
                    recoveredPath = null;
                    //string pathmaballs = direntma1.GetFullPath();
                    //if (fileNameLength == 0xE5)
                    //{
                    //    recoveredPath = FindFileInRoots(allRoots, fileName, fileSize);
                    //    if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                    //        recoveredPath = null;

                    //    if (recoveredPath == null)
                    //    {
                    //        recoveredPath = FindFileInClusteredRoots(allRoots, directoryEntryObject, fileName, fileSize);
                    //        if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                    //            recoveredPath = null;
                    //    }
                    //}
                    //else
                    //{
                    //    recoveredPath = FindFileInClusteredRoots(allRoots, directoryEntryObject, fileName, fileSize);
                    //    if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                    //        recoveredPath = null;

                    //    if (recoveredPath == null)
                    //    {
                    //        recoveredPath = FindFileInRoots(allRoots, fileName, fileSize);
                    //        if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                    //            recoveredPath = null;
                    //    }
                    string pathmaballsRecoveredBase = @"F:/hdd/Data/Recovered Data";
                    string pathmaballsRootDir = $"Cluster {direntma1.GetRootDirectoryEntry().Cluster}";
                    string pathmaballsFullPath = direntma1.GetFullPath();
                    string pathmaballs = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + pathmaballsFullPath;
                    string tempma = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + "/" + direntma1.GetFullPath();
                    if (!File.Exists(pathmaballs))
                    {
                        Console.WriteLine($"File not found: {pathmaballs}");
                        return null;
                    }
                    var fi = new FileInfo(pathmaballs);
                    if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == fileSize)
                    {
                        recoveredPath = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + pathmaballs;
                    }
                    //if the folder doesnt exist we will use the Files "Cluster" value for the Cluster {cluster} path
                    if (!Path.Exists(tempma) || !Path.Exists(pathmaballs))
                    {
                        pathmaballsRecoveredBase = @"F:/hdd/Data/Recovered Data";
                        pathmaballsRootDir = $"Cluster {direntma1.GetRootDirectoryEntry().Cluster}";
                        pathmaballsFullPath = direntma1.GetFullPath();
                        pathmaballs = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + pathmaballsFullPath;
                        tempma = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + "/" + direntma1.GetFullPath();
                        if (!Path.Exists(pathmaballs.ToString().))
                        fi = new FileInfo(pathmaballs);
                    }
                    if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == fileSize)
                    {
                        recoveredPath = pathmaballsRecoveredBase + "/" + pathmaballsRootDir + pathmaballs;
                    }
                    else
                    {
                        recoveredPath = FindFileInRoots(allRoots, fileName, fileSize);
                        if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                            recoveredPath = null;
                        if (recoveredPath == null)
                        {
                            recoveredPath = FindFileInClusteredRoots(allRoots, directoryEntryObject, fileName, fileSize);
                            if (recoveredPath != null && usedPaths.Contains(recoveredPath))
                                recoveredPath = null;
                        }
                    }
                    if (recoveredPath != null)
                    {
                        usedPaths.Add(recoveredPath);
                        using (FileStream fs = new FileStream(recoveredPath, FileMode.Open, FileAccess.Read))
                        {
                            if (directoryEntryObject.TryGetProperty("Clusters", out var clustersElement1))
                            {
                                List<uint> clusterChain = new List<uint>();
                                foreach (var clusterIndex in clustersElement1.EnumerateArray())
                                    clusterChain.Add(clusterIndex.GetUInt32());
                                byte[] buffer = new byte[this.volume.BytesPerCluster];
                                int bytesRead, totalRead = 0;

                                foreach (uint cl in clusterChain)
                                {
                                    long clusterOffset = this.volume.ClusterToPhysicalOffset(cl);
                                    bytesRead = fs.Read(buffer, 0, (int)this.volume.BytesPerCluster);
                                    this.volume.GetReader().Seek(clusterOffset);
                                    this.volume.GetReader().Write(buffer, buffer.Length);
                                    totalRead += bytesRead;
                                    if (totalRead >= fileSize)
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                this.volume.GetReader().Seek(offset);
                this.volume.GetReader().Read(data, 0x40);
            }
            var directoryEntry = new DirectoryEntry(this.volume.Platform, data, 0);
            directoryEntry.Cluster = cluster;
            directoryEntry.Offset = offset;
            bool delema = false;
            if (fileNameLength == 0xE5)
            {
                delema = true;
            }
            else
                delema = true;
            var databaseFile = fileDatabase.AddFile(directoryEntry, delema);

            // Recursively process children
            if (directoryEntryObject.TryGetProperty("Children", out var childrenElement))
            {
                foreach (var childElement in childrenElement.EnumerateArray())
                {
                    LoadDirectoryEntryFromDatabase(childElement, recoveryJson, allRoots);
                }
            }
            if (directoryEntryObject.TryGetProperty("Clusters", out var clustersElement))
            {
                List<uint> clusterChain = new List<uint>();
                foreach (var clusterIndex in clustersElement.EnumerateArray())
                {
                    clusterChain.Add(clusterIndex.GetUInt32());
                }
                databaseFile.ClusterChain = clusterChain;
            }
            return databaseFile;
        }

        // Helper to build a dirent byte[] from JSON for dirent streams
        private byte[] BuildDirentFromJson(JsonElement directoryEntryObject)
        {
            byte[] data = new byte[0x40];
            byte fileNameLength = directoryEntryObject.GetProperty("FileNameLength").GetByte();
            byte fileAttributes = directoryEntryObject.GetProperty("FileAttributes").GetByte();
            byte[] fileNameBytes = directoryEntryObject.GetProperty("FileNameBytes").GetBytesFromBase64();
            uint firstCluster = directoryEntryObject.GetProperty("FirstCluster").GetUInt32();
            uint fileSize = directoryEntryObject.GetProperty("FileSize").GetUInt32();
            uint creationTime = directoryEntryObject.GetProperty("CreationTime").GetUInt32();
            uint lastWriteTime = directoryEntryObject.GetProperty("LastWriteTime").GetUInt32();
            uint lastAccessTime = directoryEntryObject.GetProperty("LastAccessTime").GetUInt32();
            data[0x00] = fileNameLength;
            data[0x01] = fileAttributes;
            Buffer.BlockCopy(fileNameBytes, 0, data, 0x02, 0x2A);
            if (this.volume.Platform == Platform.Xbox)
            {
                Array.Copy(BitConverter.GetBytes(firstCluster), 0, data, 0x2C, 4);
                Array.Copy(BitConverter.GetBytes(fileSize), 0, data, 0x30, 4);
                Array.Copy(BitConverter.GetBytes(creationTime), 0, data, 0x34, 4);
                Array.Copy(BitConverter.GetBytes(lastWriteTime), 0, data, 0x38, 4);
                Array.Copy(BitConverter.GetBytes(lastAccessTime), 0, data, 0x3C, 4);
            }
            else // X360
            {
                byte[] fc = BitConverter.GetBytes(firstCluster); Array.Reverse(fc); Array.Copy(fc, 0, data, 0x2C, 4);
                byte[] fs = BitConverter.GetBytes(fileSize); Array.Reverse(fs); Array.Copy(fs, 0, data, 0x30, 4);
                byte[] ct = BitConverter.GetBytes(creationTime); Array.Reverse(ct); Array.Copy(ct, 0, data, 0x34, 4);
                byte[] lwt = BitConverter.GetBytes(lastWriteTime); Array.Reverse(lwt); Array.Copy(lwt, 0, data, 0x38, 4);
                byte[] lat = BitConverter.GetBytes(lastAccessTime); Array.Reverse(lat); Array.Copy(lat, 0, data, 0x3C, 4);
            }
            return data;
        }

        private DirectoryEntry BuildDirectoryEntryFromJson(JsonElement directoryEntryObject)
        {
            byte[] data = new byte[0x40];
            byte fileNameLength = directoryEntryObject.GetProperty("FileNameLength").GetByte();
            byte fileAttributes = directoryEntryObject.GetProperty("FileAttributes").GetByte();
            byte[] fileNameBytes = directoryEntryObject.GetProperty("FileNameBytes").GetBytesFromBase64();
            uint firstCluster = directoryEntryObject.GetProperty("FirstCluster").GetUInt32();
            uint fileSize = directoryEntryObject.GetProperty("FileSize").GetUInt32();
            uint creationTime = directoryEntryObject.GetProperty("CreationTime").GetUInt32();
            uint lastWriteTime = directoryEntryObject.GetProperty("LastWriteTime").GetUInt32();
            uint lastAccessTime = directoryEntryObject.GetProperty("LastAccessTime").GetUInt32();
            data[0x00] = fileNameLength;
            data[0x01] = fileAttributes;
            Buffer.BlockCopy(fileNameBytes, 0, data, 0x02, 0x2A);
            if (this.volume.Platform == Platform.Xbox)
            {
                Array.Copy(BitConverter.GetBytes(firstCluster), 0, data, 0x2C, 4);
                Array.Copy(BitConverter.GetBytes(fileSize), 0, data, 0x30, 4);
                Array.Copy(BitConverter.GetBytes(creationTime), 0, data, 0x34, 4);
                Array.Copy(BitConverter.GetBytes(lastWriteTime), 0, data, 0x38, 4);
                Array.Copy(BitConverter.GetBytes(lastAccessTime), 0, data, 0x3C, 4);
            }
            else // X360
            {
                byte[] fc = BitConverter.GetBytes(firstCluster); Array.Reverse(fc); Array.Copy(fc, 0, data, 0x2C, 4);
                byte[] fs = BitConverter.GetBytes(fileSize); Array.Reverse(fs); Array.Copy(fs, 0, data, 0x30, 4);
                byte[] ct = BitConverter.GetBytes(creationTime); Array.Reverse(ct); Array.Copy(ct, 0, data, 0x34, 4);
                byte[] lwt = BitConverter.GetBytes(lastWriteTime); Array.Reverse(lwt); Array.Copy(lwt, 0, data, 0x38, 4);
                byte[] lat = BitConverter.GetBytes(lastAccessTime); Array.Reverse(lat); Array.Copy(lat, 0, data, 0x3C, 4);
            }
            dirent = new DirectoryEntry(this.volume.Platform, data, 0);
            return dirent;
        }
        // Original function retained for compatibility
        public DatabaseFile LoadDirectoryEntryFromDatabase(JsonElement directoryEntryObject)
        {
            JsonElement offsetElement;
            if (!directoryEntryObject.TryGetProperty("Offset", out offsetElement))
            {
                Console.WriteLine($"Failed to load metadata object from database: Missing offset field");
                return null;
            }
            JsonElement clusterElement;
            if (!directoryEntryObject.TryGetProperty("Cluster", out clusterElement))
            {
                Console.WriteLine($"Failed to load metadata object from database: Missing cluster field");
                return null;
            }
            long offset = offsetElement.GetInt64();
            uint cluster = clusterElement.GetUInt32();
            if (offset < this.volume.Offset || offset > this.volume.Offset + this.volume.Length)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid offset {offset}");
                return null;
            }
            if (cluster < 0 || cluster > this.volume.MaxClusters)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid cluster {cluster}");
                return null;
            }
            byte[] data = new byte[0x40];
            DirectoryEntry Direntme = new DirectoryEntry(this.volume.Platform, data, 0);
            this.volume.GetReader().Seek(offset);
            this.volume.GetReader().Read(data, 0x40);
            var directoryEntry = new DirectoryEntry(this.volume.Platform, data, 0);
            directoryEntry.Cluster = cluster;
            directoryEntry.Offset = offset;
            var databaseFile = fileDatabase.AddFile(directoryEntry, true);

            if (directoryEntryObject.TryGetProperty("Children", out var childrenElement))
            {
                foreach (var childElement in childrenElement.EnumerateArray())
                {
                    LoadDirectoryEntryFromDatabase(childElement);
                }
            }
            if (directoryEntryObject.TryGetProperty("Clusters", out var clustersElement))
            {
                List<uint> clusterChain = new List<uint>();
                foreach (var clusterIndex in clustersElement.EnumerateArray())
                {
                    clusterChain.Add(clusterIndex.GetUInt32());
                }
                databaseFile.ClusterChain = clusterChain;
            }
            return databaseFile;
        }
        private bool LoadFromDatabase(JsonElement metadataAnalysisObject, bool recoveryJson = false, List<string> allRoots = null)
        {
            if (metadataAnalysisObject.GetArrayLength() == 0)
                return false;

            foreach (var directoryEntryObject in metadataAnalysisObject.EnumerateArray())
            {
                if (recoveryJson)
                    LoadDirectoryEntryFromDatabase(directoryEntryObject, recoveryJson, allRoots);
                else
                    LoadDirectoryEntryFromDatabase(directoryEntryObject);
            }
            return true;
        }

        public void LoadFromJson(JsonElement partitionElement, bool recoveryJson = false, List<string> allRoots = null)
        {
            fileDatabase.Reset();

            JsonElement analysisElement;
            if (!partitionElement.TryGetProperty("Analysis", out analysisElement))
            {
                var name = partitionElement.GetProperty("Name").GetString();
                throw new FileLoadException($"Database: Partition ${name} is missing Analysis object!");
            }

            if (analysisElement.TryGetProperty("MetadataAnalyzer", out var metadataAnalysisList))
            {
                if (LoadFromDatabase(metadataAnalysisList, recoveryJson, allRoots))
                {
                    FinalizeDirentStreams();
                    OnLoadRecoveryFromDatabase?.Invoke(null, null);
                    this.metadataAnalyzer = true;
                }
                else
                {
                    this.metadataAnalyzer = false;
                }
            }

            if (analysisElement.TryGetProperty("FileCarver", out var fileCarverList))
            {
                var analyzer = new FileCarver(this.volume, FileCarverInterval.Cluster, this.volume.Length);
                analyzer.LoadFromDatabase(fileCarverList);
                if (analyzer.GetCarvedFiles().Count > 0)
                {
                    view.CreateCarverView(analyzer);
                    this.fileCarver = analyzer;
                }
            }
        }
    }
}