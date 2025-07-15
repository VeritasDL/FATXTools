using FATX;
using FATX.FileSystem;
using FATXTools.DiskTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FATXTools.Database
{
    public class DriveDatabase
    {
        private readonly string driveName;
        private readonly FATX.DriveReader drive;
        private readonly FATX.DriveWriter writer;
        private List<PartitionDatabase> partitionDatabases;

        public DriveDatabase(string driveName, FATX.DriveReader drive, FATX.DriveWriter writer)
        {
            this.driveName = driveName;
            this.drive = drive;
            this.writer = writer;
            partitionDatabases = new List<PartitionDatabase>();
        }

        public event EventHandler<AddPartitionEventArgs> OnPartitionAdded;
        public event EventHandler<RemovePartitionEventArgs> OnPartitionRemoved;

        public PartitionDatabase AddPartition(Volume volume)
        {
            var partitionDatabase = new PartitionDatabase(volume);
            partitionDatabases.Add(partitionDatabase);
            return partitionDatabase;
        }

        public void RemovePartition(int index)
        {
            partitionDatabases.RemoveAt(index);

            OnPartitionRemoved?.Invoke(this, new RemovePartitionEventArgs(index));
        }

        public void Save(string path)
        {
            Dictionary<string, object> databaseObject = new Dictionary<string, object>();

            databaseObject["Version"] = 1;
            databaseObject["Drive"] = new Dictionary<string, object>();

            var driveObject = databaseObject["Drive"] as Dictionary<string, object>;
            driveObject["FileName"] = driveName;

            driveObject["Partitions"] = new List<Dictionary<string, object>>();
            var partitionList = driveObject["Partitions"] as List<Dictionary<string, object>>;

            foreach (var partitionDatabase in partitionDatabases)
            {
                var partitionObject = new Dictionary<string, object>();
                partitionList.Add(partitionObject);
                partitionDatabase.Save(partitionObject);
            }

            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };

            string json = JsonSerializer.Serialize(databaseObject, jsonSerializerOptions);

            File.WriteAllText(path, json);
        }

        // Update LoadIfNotExists signature and call
        private bool LoadIfNotExists(JsonElement partitionElement, bool recoveryJson, List<string> allRoots)
        {
            foreach (var partitionDatabase in partitionDatabases)
            {
                if (partitionDatabase.Volume.Offset == partitionElement.GetProperty("Offset").GetInt64())
                {
                    partitionDatabase.LoadFromJson(partitionElement, recoveryJson, allRoots);
                    return true;
                }
            }
            return false;
        }
        public void LoadFromJson(string path, bool recoveryJson, List<string> allRoots = null)
        {
            string json = File.ReadAllText(path);
            Dictionary<string, object> databaseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (databaseObject.ContainsKey("Drive"))
            {
                JsonElement driveJsonElement = (JsonElement)databaseObject["Drive"];
                if (driveJsonElement.TryGetProperty("Partitions", out var partitionsElement))
                {
                    foreach (var partitionElement in partitionsElement.EnumerateArray())
                    {
                        // Check if partition exists
                        if (!LoadIfNotExists(partitionElement, recoveryJson, allRoots))
                        {
                            // It does not exist, let's load it in.
                            var offset = partitionElement.GetProperty("Offset").GetInt64();
                            var length = partitionElement.GetProperty("Length").GetInt64();
                            var name = partitionElement.GetProperty("Name").GetString();

                            Volume newVolume = new Volume(this.drive, this.writer, name, offset, length);
                            OnPartitionAdded?.Invoke(this, new AddPartitionEventArgs(newVolume));
                            partitionDatabases[partitionDatabases.Count - 1].LoadFromJson(partitionElement, recoveryJson, allRoots);
                        }
                    }
                }
                else
                {
                    throw new FileLoadException("Database: Drive has no Partition list!");
                }
            }
            else
            {
                throw new FileLoadException("Database: Missing Drive object!");
            }
        }
    }
}
