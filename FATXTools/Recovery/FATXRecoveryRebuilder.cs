using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FATX;
using FATX.FileSystem;
using FATXTools.Database;

namespace FATXTools.Recovery
{
    public static class FATXRecoveryRebuilder
    {
        public delegate void ProgressDelegate(string message, int status);
        public static bool isdel;
        // Class-level set for single-use file enforcement
        private static HashSet<string> usedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public static DirectoryEntry DirectoryEntryFromJson(JsonElement entry)
        {
            byte[] bytes = new byte[0x40];
            bytes[0x00] = entry.GetProperty("FileNameLength").GetByte();
            bytes[0x01] = entry.GetProperty("FileAttributes").GetByte();
            var fileNameBytes = Convert.FromBase64String(entry.GetProperty("FileNameBytes").GetString() ?? "");
            int toCopy = Math.Min(fileNameBytes.Length, 42);
            Array.Clear(bytes, 0x02, 42);
            Array.Copy(fileNameBytes, 0, bytes, 0x02, toCopy);
            var fields = new[] {
                (entry.GetProperty("FirstCluster").GetUInt32(), 0x2C),
                (entry.GetProperty("FileSize").GetUInt32(), 0x30),
                (entry.GetProperty("CreationTime").GetUInt32(), 0x34),
                (entry.GetProperty("LastWriteTime").GetUInt32(), 0x38),
                (entry.GetProperty("LastAccessTime").GetUInt32(), 0x3C)
            };
            foreach (var (val, offset) in fields)
            {
                byte[] temp = BitConverter.GetBytes(val);
                if (BitConverter.IsLittleEndian) Array.Reverse(temp);
                Array.Copy(temp, 0, bytes, offset, 4);
            }
            return new DirectoryEntry(Platform.X360, bytes, 0);
        }

        public static void WriteDirentToImage(DirectoryEntry dirent, Stream imageStream, long offset, Action<string> logCallback = null)
        {
            byte[] bytes = new byte[0x40];
            dirent.WriteTo(bytes, 0);
            imageStream.Seek(offset, SeekOrigin.Begin);
            imageStream.Write(bytes, 0, bytes.Length);
            Console.WriteLine($"dirent: {offset:X8}: {dirent.FileName}");
        }

        // Finds and reserves a file by name and size (single-use per session)
        public static string FindAndReserveRecoveredFile(string fileName, long fileSize, IEnumerable<string> searchRoots)
        {
            foreach (var root in searchRoots)
            {
                foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var fi = new FileInfo(file);
                        if (!usedFiles.Contains(file) &&
                            fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) &&
                            fi.Length == fileSize)
                        {
                            usedFiles.Add(file);
                            Console.WriteLine($"Matched: {file}, {fileSize}");
                            return file;
                        }
                    }
                    catch { }
                }
            }
            return null;
        }

        // Restore one DirectoryEntry + file data, recursive for children
        public static DatabaseFile RestoreDirectoryEntryFromJson(JsonElement entry, FileDatabase fileDatabase, Stream imageStream, List<string> recoveredFolders, long fileAreaOffset, uint clusterSize, ProgressDelegate progressCallback = null, Action<string> logCallback = null)
        {
            if (!entry.TryGetProperty("Offset", out var offsetElement) || !entry.TryGetProperty("Cluster", out var clusterElement))
            {
                Console.WriteLine("Missing Offset/Cluster in entry");
                return null;
            }
            long offset = offsetElement.GetInt64();
            uint cluster = clusterElement.GetUInt32();
            // Always use FileNameLength==0xE5 for missing, but keep everything else as-is
            bool forceDeleted = false;

            // Deleted file logic: If FileNameLength==0xE5, check all "Recovered Data" roots; else, check "Non Deleted Data"
            var fileNameLength = entry.GetProperty("FileNameLength").GetByte();
            string fileName = entry.GetProperty("FileName").GetString();
            long fileSize = entry.GetProperty("FileSize").GetInt64();
            if (fileNameLength == 0xE5)
            {
                isdel = true;
            }
            else
            {
                isdel = false;
            }
            bool isDir = entry.GetProperty("FileAttributes").GetByte() == 0x10;
            string foundFile = null;

            if (!isDir && fileSize > 0)
            {
                // If deleted entry, search all recoveredFolders; else, prefer "Non Deleted Data" (user should order the list accordingly)
                foundFile = FindAndReserveRecoveredFile(fileName, fileSize, recoveredFolders);

                if (foundFile == null)
                {
                    //forceDeleted = true;
                    progressCallback?.Invoke($"FILE NOT FOUND: {fileName} ({fileSize}", -1);
                    //Console.WriteLine($"FILE NOT FOUND: {fileName} ({fileSize})");
                }
            }
            // Write dirent
            var dirent = DirectoryEntryFromJson(entry);
            dirent.Cluster = cluster;
            dirent.Offset = offset;
            WriteDirentToImage(dirent, imageStream, offset, logCallback);
            DatabaseFile dbFile = fileDatabase.AddFile(dirent, isdel);

            // Set cluster chain if present
            if (entry.TryGetProperty("Clusters", out var clustersElement))
            {
                List<uint> clusterChain = new List<uint>();
                foreach (var clusterIndex in clustersElement.EnumerateArray())
                {
                    clusterChain.Add(clusterIndex.GetUInt32());
                }
                dbFile.ClusterChain = clusterChain;
            }


            // Restore file data if present and not missing/deleted
            if (!isDir && !forceDeleted && foundFile != null)
            {
                try
                {
                    RestoreFileDataToImage(foundFile, entry, imageStream, fileAreaOffset, clusterSize, progressCallback);
                }
                catch (Exception ex)
                {
                    progressCallback?.Invoke($"RESTORE FAIL: {fileName}: {ex.Message}", -1);
                    Console.WriteLine($"RESTORE FAIL: {fileName}: {ex.Message}");
                }
            }

            // Process children recursively
            if (entry.TryGetProperty("Children", out var childrenElement) && childrenElement.ValueKind == JsonValueKind.Array)
            {
                dbFile.Children = new List<DatabaseFile>();
                foreach (var child in childrenElement.EnumerateArray())
                {
                    var childDbFile = RestoreDirectoryEntryFromJson(child, fileDatabase, imageStream, recoveredFolders, fileAreaOffset, clusterSize, progressCallback, logCallback);
                    if (childDbFile != null)
                        dbFile.Children.Add(childDbFile);
                }
            }
            return dbFile;
        }

        private static void RestoreFileDataToImage(string foundFile, JsonElement entry, Stream imageStream, long fileAreaOffset, uint clusterSize, ProgressDelegate progressCallback)
        {
            long fileSize = entry.GetProperty("FileSize").GetInt64();
            var clusters = entry.GetProperty("Clusters").EnumerateArray();
            using (FileStream src = File.OpenRead(foundFile))
            {
                long bytesRemaining = fileSize;
                byte[] buf = new byte[clusterSize];
                foreach (var clusterVal in clusters)
                {
                    int cluster = clusterVal.GetInt32();
                    int toRead = (int)Math.Min(clusterSize, bytesRemaining);
                    int read = src.Read(buf, 0, toRead);
                    if (read != toRead)
                    {
                        progressCallback?.Invoke($"READ FAIL: {foundFile}", -1);
                        Console.WriteLine($"READ FAIL: {foundFile}");
                        return;
                    }
                    long writeOffset = fileAreaOffset + (long)(cluster - 1) * clusterSize;
                    imageStream.Seek(writeOffset, SeekOrigin.Begin);
                    imageStream.Write(buf, 0, read);
                    //Console.WriteLine($"Wrote {cluster} at offset {writeOffset:X}");
                    bytesRemaining -= read;
                    if (bytesRemaining <= 0) break;
                }
            }
            progressCallback?.Invoke($"WROTE: {foundFile}", 1);
            Console.WriteLine($"WROTE: {foundFile}");
        }

        /// <summary>
        /// Main entry: restore a partition (entire MetadataAnalyzer) from JSON, writing to image and populating FileDatabase.
        /// Returns a list of top-level DatabaseFile objects (roots).
        /// </summary>
        public static List<DatabaseFile> RestorePartitionFromJson(JsonElement partitionElement, Volume volume, FileDatabase fileDatabase, List<string> recoveredFolders, ProgressDelegate progressCallback = null, Action<string> logCallback = null)
        {
            usedFiles.Clear();

            long fileAreaOffset = volume.FileAreaByteOffset + volume.Offset;
            var imageStream = volume.GetWriter().BaseStream;
            uint clusterSize = volume.BytesPerCluster;

            var restoredRoots = new List<DatabaseFile>();

            if (partitionElement.TryGetProperty("Analysis", out var analysisProp) &&
                analysisProp.TryGetProperty("MetadataAnalyzer", out var metadataAnalyzerArr) &&
                metadataAnalyzerArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var rootDirEntry in metadataAnalyzerArr.EnumerateArray())
                {
                    var dbFile = RestoreDirectoryEntryFromJson(rootDirEntry, fileDatabase, imageStream, recoveredFolders, fileAreaOffset, clusterSize, progressCallback, logCallback);
                    if (dbFile != null)
                        restoredRoots.Add(dbFile);
                }
            }
            Console.WriteLine("Finished partition restore.");
            return restoredRoots;
        }
    }
}
