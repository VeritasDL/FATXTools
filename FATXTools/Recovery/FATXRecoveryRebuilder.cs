using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FATX.FileSystem;

namespace FATXTools.Recovery
{
    public static class FATXRecoveryRebuilder
    {
        public delegate void ProgressDelegate(string message, int status);

        public static Platform platform = Platform.X360;

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
            return new DirectoryEntry(platform, bytes, 0);
        }

        public static void WriteDirentToImage(DirectoryEntry dirent, Stream imageStream, long offset, Action<string> logCallback = null)
        {
            byte[] bytes = new byte[0x40];
            dirent.WriteTo(bytes, 0);
            imageStream.Seek(offset, SeekOrigin.Begin);
            imageStream.Write(bytes, 0, bytes.Length);
            logCallback?.Invoke($"0x{offset:X8}: {dirent.FileName}");
        }

        public static void RestoreFileDataFromJson(
            JsonElement entry,
            List<string> recoveredBasePaths,
            Stream imageStream,
            long fileAreaOffset,
            uint clusterSize = 0x4000,
            ProgressDelegate progressCallback = null)
        {
            if (entry.GetProperty("FileAttributes").GetInt32() == 0x10) return;
            string fileName = entry.GetProperty("FileName").GetString();
            long fileSize = entry.GetProperty("FileSize").GetInt64();
            if (fileSize == 0) return;
            var clusters = entry.GetProperty("Clusters").EnumerateArray().Select(x => x.GetInt32()).ToList();

            string foundFilePath = null;
            foreach (var basePath in recoveredBasePaths)
            {
                foundFilePath = FindRecoveredFile(fileName, fileSize, basePath);
                if (foundFilePath != null) break;
            }

            if (foundFilePath == null)
            {
                progressCallback?.Invoke($"MISSING DATA: {fileName} ({fileSize} bytes)", -1);
                return;
            }

            using (FileStream src = File.OpenRead(foundFilePath))
            {
                long bytesRemaining = fileSize;
                byte[] buf = new byte[clusterSize];
                foreach (int cluster in clusters)
                {
                    int toRead = (int)Math.Min(clusterSize, bytesRemaining);
                    int read = src.Read(buf, 0, toRead);
                    if (read != toRead)
                    {
                        progressCallback?.Invoke($"READ FAIL: {fileName}", -1);
                        return;
                    }
                    long writeOffset = fileAreaOffset + (long)(cluster - 1) * clusterSize;
                    imageStream.Seek(writeOffset, SeekOrigin.Begin);
                    imageStream.Write(buf, 0, read);
                    bytesRemaining -= read;
                    if (bytesRemaining <= 0) break;
                }
            }
            progressCallback?.Invoke($"WROTE: {fileName}", 1);
        }

        public static void RestoreTreeFromJson(
            JsonElement entry,
            Stream imageStream,
            List<string> recoveredBasePaths,
            long fileAreaOffset,
            uint clusterSize,
            ProgressDelegate progressCallback = null,
            Action<string> logCallback = null)
        {
            var dirent = DirectoryEntryFromJson(entry);
            long offset = entry.GetProperty("Offset").GetInt64();
            WriteDirentToImage(dirent, imageStream, offset, logCallback);

            if (entry.GetProperty("FileAttributes").GetInt32() != 0x10)
            {
                try
                {
                    RestoreFileDataFromJson(entry, recoveredBasePaths, imageStream, fileAreaOffset, clusterSize, progressCallback);
                }
                catch (Exception ex)
                {
                    progressCallback?.Invoke($"DATA RESTORE FAIL: {dirent.FileName}: {ex.Message}", -1);
                }
            }
            if (entry.TryGetProperty("Children", out var childrenProp) && childrenProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in childrenProp.EnumerateArray())
                    RestoreTreeFromJson(child, imageStream, recoveredBasePaths, fileAreaOffset, clusterSize, progressCallback, logCallback);
            }
        }

        public static string FindRecoveredFile(string fileName, long fileSize, string basePath)
        {
            foreach (var file in Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    var fi = new FileInfo(file);
                    if (fi.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase) && fi.Length == fileSize)
                        return file;
                }
                catch { }
            }
            return null;
        }

        public static void RestorePartitionFromJson(
            JsonElement partitionElement,
            Volume volume,
            List<string> recoveredBasePaths,
            ProgressDelegate progressCallback = null,
            Action<string> logCallback = null)
        {
            // Use dynamic fileAreaOffset for safety and compatibility
            long fileAreaOffset = volume.FileAreaByteOffset + volume.Offset;
            uint clusterSize = volume.BytesPerCluster;

            var imageStream = volume.GetWriter().BaseStream;

            if (partitionElement.TryGetProperty("Analysis", out var analysisProp) &&
                analysisProp.TryGetProperty("MetadataAnalyzer", out var metadataAnalyzerArr) &&
                metadataAnalyzerArr.ValueKind == JsonValueKind.Array)
            {
                foreach (var rootDirEntry in metadataAnalyzerArr.EnumerateArray())
                {
                    RestoreTreeFromJson(
                        rootDirEntry,
                        imageStream,
                        recoveredBasePaths,
                        fileAreaOffset,
                        clusterSize,
                        progressCallback,
                        logCallback
                    );
                }
            }
        }
    }
}
