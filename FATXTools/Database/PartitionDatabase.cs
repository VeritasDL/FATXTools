using FATX;
using FATX.Analyzers;
using FATX.FileSystem;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace FATXTools.Database
{
    public class PartitionDatabase
    {
        // We just want this for the volume info (offset, length, name)
        Volume volume;
        private List<string> RecoveryLog = new List<string>();
        FileCarver fileCarver;  // TODO: Get rid of this. We should be able to get this information from the FileDatabase.
        private const string RecoveryLogFilePath = "FATxToolRecovery_Log.txt";
        /// <summary>
        ///  Whether or not the MetadataAnalyzer's results are in.
        /// </summary>
        bool metadataAnalyzer;

        /// <summary>
        /// Database that stores analysis results.
        /// </summary>
        FileDatabase fileDatabase;

        /// <summary>
        /// The associated view for this PartitionDatabase
        /// </summary>
        PartitionView view; // TODO: Use events instead.

        public event EventHandler OnLoadRecoveryFromDatabase;

        public PartitionDatabase(Volume volume)
        {
            this.volume = volume;
            this.metadataAnalyzer = false;
            this.fileCarver = null;

            this.fileDatabase = new FileDatabase(volume);
        }

        /// <summary>
        /// Get the partition name associated with this database.
        /// </summary>
        public string PartitionName => volume.Name;

        public Volume Volume => volume;

        /// <summary>
        /// Set the associated view for this database.
        /// </summary>
        /// <param name="view"></param>
        public void SetPartitionView(PartitionView view)
        {
            this.view = view;
        }

        /// <summary>
        /// Set whether or not analysis was performed.
        /// </summary>
        /// <param name="metadataAnalyzer"></param>
        public void SetMetadataAnalyzer(bool metadataAnalyzer)
        {
            this.metadataAnalyzer = metadataAnalyzer;
        }

        public void SetFileCarver(FileCarver fileCarver)
        {
            this.fileCarver = fileCarver;
        }

        /// <summary>
        /// Get the FileDatabase for this partition.
        /// </summary>
        /// <returns></returns>
        public FileDatabase GetFileDatabase()
        {
            return this.fileDatabase;
        }

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
        private void AddRecoveryLog(string message)
        {
            RecoveryLog.Add(message);
            Console.WriteLine(message);
            // Save to file
            //File.AppendAllLines(RecoveryLogFilePath, RecoveryLog);
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

            /*
             * At this moment, I believe this will only be used for debugging.
             */
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

        public DatabaseFile LoadDirectoryEntryFromDatabase(JsonElement directoryEntryObject)
        {
            // Make sure that the offset property was stored for this file
            JsonElement offsetElement;
            if (!directoryEntryObject.TryGetProperty("Offset", out offsetElement))
            {
                Console.WriteLine("Failed to load metadata object from database: Missing offset field");
                return null;
            }

            // Make sure that the cluster property was stored for this file
            JsonElement clusterElement;
            if (!directoryEntryObject.TryGetProperty("Cluster", out clusterElement))
            {
                Console.WriteLine("Failed to load metadata object from database: Missing cluster field");
                return null;
            }

            long offset = offsetElement.GetInt64();
            uint cluster = clusterElement.GetUInt32();

            // Ensure that the offset is within the bounds of the partition.
            if (offset < this.volume.Offset || offset > this.volume.Offset + this.volume.Length)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid offset {offset}");
                return null;
            }

            // Ensure that the cluster index is valid
            if (cluster < 0 || cluster > this.volume.MaxClusters)
            {
                Console.WriteLine($"Failed to load metadata object from database: Invalid cluster {cluster}");
                return null;
            }

            // Read the DirectoryEntry data
            byte[] data = new byte[0x40];
            this.volume.GetReader().Seek(offset);
            this.volume.GetReader().Read(data, 0x40);

            // Create a DirectoryEntry
            var directoryEntry = new DirectoryEntry(this.volume.Platform, data, 0);
            directoryEntry.Cluster = cluster;
            directoryEntry.Offset = offset;

            // Add this file to the FileDatabase
            var databaseFile = fileDatabase.AddFile(directoryEntry, true);

            /*
             * Here we begin assigning our user-configurable modifications
             */

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

                //if (databaseFile.FileName == "ears_godfather")
                //    System.Diagnostics.Debugger.Break();

                foreach (var clusterIndex in clustersElement.EnumerateArray())
                {
                    clusterChain.Add(clusterIndex.GetUInt32());
                }

                databaseFile.ClusterChain = clusterChain;
            }

            return databaseFile;
        }
        private void RestoreDirectoryEntryFromDatabase(JsonElement entry, string recoveredFolder, string relPath)
        {
            var volume = this.volume;
            var driveReader = volume.GetReader();
            var endianWriter = volume.GetWriter();
            // Read common FATX directory entry fields
            var offset = entry.GetProperty("Offset").GetInt64();
            var fileName = entry.GetProperty("FileName").GetString();
            var fileAttributes = entry.GetProperty("FileAttributes").GetByte();
            var fileNameBytesB64 = entry.GetProperty("FileNameBytes").GetString();
            var fileNameBytes = Convert.FromBase64String(fileNameBytesB64);
            var fileNameLength = entry.GetProperty("FileNameLength").GetByte();
            var firstCluster = entry.GetProperty("FirstCluster").GetUInt32();
            var fileSize = entry.GetProperty("FileSize").GetUInt32();
            var creationTime = entry.GetProperty("CreationTime").GetUInt32();
            var lastWriteTime = entry.GetProperty("LastWriteTime").GetUInt32();
            var lastAccessTime = entry.GetProperty("LastAccessTime").GetUInt32();

            // Compose directory entry (0x40 bytes)
            var dirent = new byte[0x40];
            dirent[0x00] = fileNameLength;
            dirent[0x01] = fileAttributes;
            Buffer.BlockCopy(fileNameBytes, 0, dirent, 0x02, 0x2A); // 42 bytes for FileNameBytes at offset 0x02

            // Write fields in big endian (as per Xbox 360)
            BinaryPrimitives.WriteUInt32BigEndian(dirent.AsSpan(0x2C, 4), firstCluster);
            BinaryPrimitives.WriteUInt32BigEndian(dirent.AsSpan(0x30, 4), fileSize);
            BinaryPrimitives.WriteUInt32BigEndian(dirent.AsSpan(0x34, 4), creationTime);
            BinaryPrimitives.WriteUInt32BigEndian(dirent.AsSpan(0x38, 4), lastWriteTime);
            BinaryPrimitives.WriteUInt32BigEndian(dirent.AsSpan(0x3C, 4), lastAccessTime);

            // Write directory entry to correct offset in image/device
            driveReader.Seek(offset, SeekOrigin.Begin);
            endianWriter.Write(dirent, dirent.Length);

            // If directory, recurse into children
            if ((fileAttributes & 0x10) != 0)
            {
                if (entry.TryGetProperty("Children", out var children))
                {
                    foreach (var child in children.EnumerateArray())
                    {
                        RestoreDirectoryEntryFromDatabase(child, recoveredFolder, Path.Combine(relPath, fileName));
                    }
                }
            }
            else // File: copy data to the correct clusters
            {
                if (entry.TryGetProperty("Clusters", out var clusters))
                {
                    // Source file is in e.g. Recovered Data\Cluster N\...\FileName
                    string clusterDir = Path.Combine(recoveredFolder, $"Cluster {entry.GetProperty("Cluster").GetInt32()}", relPath);
                    string sourceFile = Path.Combine(clusterDir, fileName);
                    if (!File.Exists(sourceFile))
                    {
                        Console.WriteLine($"Missing recovered file: {sourceFile}");
                        return;
                    }
                    byte[] fileData = File.ReadAllBytes(sourceFile);

                    int clusterSize = (int)volume.BytesPerCluster;
                    int bytesWritten = 0;
                    foreach (var clusterVal in clusters.EnumerateArray())
                    {
                        uint clusterNum = clusterVal.GetUInt32();
                        long clusterOffset = volume.ClusterToPhysicalOffset(clusterNum);
                        driveReader.Seek(clusterOffset, SeekOrigin.Begin);
                        endianWriter.Write(fileData, bytesWritten, clusterSize);
                        bytesWritten += clusterSize;
                        if (bytesWritten >= fileData.Length)
                            break;
                    }
                }
            }
        }
        public void RecoverFromDatabase(JsonElement metadataAnalysisObject, string recoveredFolder)
        {
            foreach (var directoryEntryObject in metadataAnalysisObject.EnumerateArray())
            {
                RestoreDirectoryEntryFromDatabase(directoryEntryObject, recoveredFolder, "");
            }
        }
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
        public void RecoverFromJson(JsonElement partitionElement, string recoveredFolder)
        {
            // Similar to LoadFromJson, but call FATXRecoveryRebuilder
            fileDatabase.Reset();

            if (!partitionElement.TryGetProperty("Analysis", out var analysisElement))
            {
                var name = partitionElement.GetProperty("Name").GetString();
                throw new FileLoadException($"Database: Partition {name} is missing Analysis object");
            }

            if (analysisElement.TryGetProperty("MetadataAnalyzer", out var metadataAnalysisList))
            {
                // --- Key difference: restore from JSON using Rebuilder ---
                // You'll need access to the image stream and correct parameters
                var imgStream = this.Volume.GetWriter().BaseStream;
                long fileAreaOffset = 0x2865BC000;
                uint clusterSize = this.Volume.BytesPerCluster;
                var basePaths = new List<string> { recoveredFolder };

                foreach (var entry in metadataAnalysisList.EnumerateArray())
                {
                    FATXTools.Recovery.FATXRecoveryRebuilder.RestoreTreeFromJson(
                        entry,
                        imgStream,
                        basePaths,
                        fileAreaOffset,
                        clusterSize
                    // Progress/reporting/callbacks can be added here if needed
                    );
                }
                this.metadataAnalyzer = true;
                OnLoadRecoveryFromDatabase?.Invoke(null, null);
            }
            // FileCarver code is omitted for brevity (not needed for restore)
        }

        private bool LoadFromDatabase(JsonElement metadataAnalysisObject)
        {
            if (metadataAnalysisObject.GetArrayLength() == 0)
                return false;

            // Load each root file and its children from the json database
            foreach (var directoryEntryObject in metadataAnalysisObject.EnumerateArray())
            {
                LoadDirectoryEntryFromDatabase(directoryEntryObject);
            }

            return true;
        }

        public void LoadFromJson(JsonElement partitionElement)
        {
            // We are loading a new database so clear previous results
            fileDatabase.Reset();

            // Find the Analysis element, which contains analysis results
            JsonElement analysisElement;
            if (!partitionElement.TryGetProperty("Analysis", out analysisElement))
            {
                var name = partitionElement.GetProperty("Name").GetString();
                throw new FileLoadException($"Database: Partition ${name} is missing Analysis object");
            }

            if (analysisElement.TryGetProperty("MetadataAnalyzer", out var metadataAnalysisList))
            {
                // Loads the files from the json into the FileDatabase
                // Only post the results if there actually was any
                if (LoadFromDatabase(metadataAnalysisList))
                {
                    OnLoadRecoveryFromDatabase?.Invoke(null, null);

                    // Mark that analysis was done
                    this.metadataAnalyzer = true;
                }
                else
                {
                    // Element was there but no analysis results were loaded
                    this.metadataAnalyzer = false;
                }
            }

            if (analysisElement.TryGetProperty("FileCarver", out var fileCarverList))
            {
                // TODO: We will begin replacing this when we start work on customizable "CarvedFiles"
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
