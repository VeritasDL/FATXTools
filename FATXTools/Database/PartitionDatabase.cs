using FATX;
using FATX.Analyzers;
using FATX.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FATXTools.Database
{
    public class PartitionDatabase
    {
        Volume volume;
        FileCarver fileCarver;  // TODO: Get rid of this. We should be able to get this information from the FileDatabase.
        bool metadataAnalyzer;
        FileDatabase fileDatabase;
        PartitionView view; // TODO: Use events instead.
        DriveWriter _writer;

        public event EventHandler OnLoadRecoveryFromDatabase;

        public PartitionDatabase(Volume volume)
        {
            this.volume = volume;
            this.metadataAnalyzer = false;
            this.fileCarver = null;
            this.fileDatabase = new FileDatabase(volume);
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
        public DatabaseFile LoadDirectoryEntryFromDatabase(JsonElement directoryEntryObject, bool recoveryJson, List<string> allRoots)
        {
            JsonElement offsetElement;
            _writer = this.volume.GetWriter();
            if (_writer == null)
            {
                Console.WriteLine("Writer is null, cannot write to disk.");
                if (this.volume.GetReader().BaseStream != null)
                {
                    _writer = new DriveWriter(this.volume.GetReader().BaseStream);
                }
                else
                {
                    return null; // Cannot proceed without a valid writer
                }
            }
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
                //this.volume.GetWriter().Seek(offset);
                if(this.volume.GetWriter() == null)
                {
                    Console.WriteLine("Writer is null, cannot write to disk.");
                    if (this.volume.GetReader().BaseStream != null)
                    {
                        _writer = new DriveWriter(this.volume.GetReader().BaseStream);
                        _writer.Write(data, 0x40);
                    }
                }
                else
                {
                    this.volume.GetWriter().Write(data, 0x40);
                }


                bool isDirectory = (fileAttributes & 0x10) != 0;
                if (isDirectory)
                {
                    // Write the dirent stream for all children to FirstCluster
                    List<JsonElement> childrenList = new List<JsonElement>();
                    if (directoryEntryObject.TryGetProperty("Children", out var childrenElement1))
                    {
                        foreach (var childElement in childrenElement1.EnumerateArray())
                            childrenList.Add(childElement);
                    }

                    int bytesPerCluster = (int)this.volume.BytesPerCluster;
                    int maxDirents = bytesPerCluster / 0x40; // 256

                    byte[] direntCluster = new byte[bytesPerCluster];
                    for (int i = 0; i < childrenList.Count && i < maxDirents; i++)
                    {
                        byte[] childDirent = BuildDirentFromJson(childrenList[i]);
                        Buffer.BlockCopy(childDirent, 0, direntCluster, i * 0x40, 0x40);
                    }
                    // Pad remaining dirents with 0xFF
                    for (int i = childrenList.Count; i < maxDirents; i++)
                        for (int j = 0; j < 0x40; j++)
                            direntCluster[i * 0x40 + j] = 0xFF;

                    // Write dirent stream to firstCluster
                    long dirClusterOffset = this.volume.ClusterToPhysicalOffset(firstCluster);
                    this.volume.GetReader().Seek(dirClusterOffset);
                    //this.volume.GetWriter().Seek(offset);
                    if (this.volume.GetWriter() == null)
                    {
                        if (this.volume.GetReader().BaseStream != null)
                        {
                            _writer = new DriveWriter(this.volume.GetReader().BaseStream);
                            _writer.Write(direntCluster, bytesPerCluster);
                        }
                    }
                    else
                    {
                        volume.GetWriter().Write(direntCluster, bytesPerCluster);
                    }

                    
                }
                else // file
                {
                    string recoveredPath = allRoots.FirstOrDefault(f =>
                        Path.GetFileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                        new FileInfo(f).Length == fileSize);

                    if (recoveredPath != null)
                    {
                        if (directoryEntryObject.TryGetProperty("Clusters", out var clustersElement1))
                        {
                            List<uint> clusterChain = new List<uint>();
                            foreach (var clusterIndex in clustersElement1.EnumerateArray())
                                clusterChain.Add(clusterIndex.GetUInt32());

                            int bytesPerCluster = (int)this.volume.BytesPerCluster;
                            int expectedClusters = (int)((fileSize + bytesPerCluster - 1) / bytesPerCluster);

                            if (clusterChain.Count != expectedClusters)
                            {
                                Console.WriteLine($"Cluster mismatch {fileName}: expected {expectedClusters}, got {clusterChain.Count}. Skipping file.");
                            }
                            else
                            {
                                int fileOffset = 0;
                                using (FileStream fs = new FileStream(recoveredPath, FileMode.Open, FileAccess.Read))
                                {
                                    byte[] buffer = new byte[bytesPerCluster];
                                    for (int i = 0; i < clusterChain.Count; i++)
                                    {
                                        uint cl = clusterChain[i];
                                        long clusterOffset = this.volume.ClusterToPhysicalOffset(cl);

                                        int bytesRemaining = (int)Math.Min(bytesPerCluster, fileSize - fileOffset);

                                        if (bytesRemaining > 0)
                                        {
                                            int bytesRead = fs.Read(buffer, 0, bytesRemaining);
                                            this.volume.GetReader().Seek(clusterOffset);

                                            if (this.volume.GetWriter() == null)
                                            {
                                                if (this.volume.GetReader().BaseStream != null)
                                                {
                                                    _writer = new DriveWriter(this.volume.GetReader().BaseStream);
                                                    _writer.Write(buffer, bytesRead);
                                                }
                                            }
                                            else
                                            {
                                                this.volume.GetWriter().Write(buffer, bytesRead);
                                            }
                                            fileOffset += bytesRead;
                                        }
                                    }
                                }
                                Console.WriteLine($"{fileName} {fileSize} found ");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{fileName} {fileSize} not found or size mismatch");
                    }
                }
            }
            else
            {
                // Original logic: read the dirent from disk
                this.volume.GetReader().Seek(offset);
                this.volume.GetReader().Read(data, 0x40);
            }

            var directoryEntry = new DirectoryEntry(this.volume.Platform, data, 0);
            directoryEntry.Cluster = cluster;
            directoryEntry.Offset = offset;
            var databaseFile = fileDatabase.AddFile(directoryEntry, true);

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

        // Original function retained for compatibility
        public DatabaseFile LoadDirectoryEntryFromDatabase(JsonElement directoryEntryObject)
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
        public DriveWriter GetWriter()
        {
            return _writer;
        }
    }
}