﻿using FATX;
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
        string driveName;
        DriveReader drive;
        List<PartitionDatabase> partitionDatabases;

        public DriveDatabase(string driveName, DriveReader drive)
        {
            this.driveName = driveName;
            this.drive = drive;

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

        private bool LoadIfNotExists(JsonElement partitionElement)
        {
            foreach (var partitionDatabase in partitionDatabases)
            {
                if (partitionDatabase.Volume.Offset == partitionElement.GetProperty("Offset").GetInt64())
                {
                    partitionDatabase.LoadFromJson(partitionElement);

                    return true;
                }
            }

            return false;
        }

        private bool RecoverIfNotExists(JsonElement partitionElement, string recoveredFolder)
        {
            foreach (var partitionDatabase in partitionDatabases)
            {
                if (partitionDatabase.Volume.Offset == partitionElement.GetProperty("Offset").GetInt64())
                {
                    partitionDatabase.RecoverFromJson(partitionElement, recoveredFolder);
                    return true;
                }
            }
            return false;
        }


        public void LoadFromJson(string path)
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
                        if (!LoadIfNotExists(partitionElement))
                        {
                            // It does not exist, let's load it in.
                            var offset = partitionElement.GetProperty("Offset").GetInt64();
                            var length = partitionElement.GetProperty("Length").GetInt64();
                            var name = partitionElement.GetProperty("Name").GetString();
                            Volume newVolume = new Volume(this.drive, name, offset, length);

                            OnPartitionAdded?.Invoke(this, new AddPartitionEventArgs(newVolume));

                            // Might need some clean up here. Should not rely on the event to add the partition to the database.
                            partitionDatabases[partitionDatabases.Count - 1].LoadFromJson(partitionElement);
                        }
                    }

                }
                else
                {
                    throw new FileLoadException("Database: Drive has no Partition list");
                }
            }
            else
            {
                throw new FileLoadException("Database: Missing Drive object");
            }
        }
        public void RecoverFromJson(string jsonPath, string recoveredFolder)
        {
            string json = File.ReadAllText(jsonPath);
            Dictionary<string, object> databaseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            if (databaseObject.ContainsKey("Drive"))
            {
                JsonElement driveJsonElement = (JsonElement)databaseObject["Drive"];
                if (driveJsonElement.TryGetProperty("Partitions", out var partitionsElement))
                {
                    foreach (var partitionElement in partitionsElement.EnumerateArray())
                    {
                        // Check if partition exists for recovery
                        if (!RecoverIfNotExists(partitionElement, recoveredFolder) || drive.Partitions.Count == 0)
                        {
                            // It does not exist, let's recover it in.
                            var offset = partitionElement.GetProperty("Offset").GetInt64();
                            var length = partitionElement.GetProperty("Length").GetInt64();
                            var name = partitionElement.GetProperty("Name").GetString();
                            Volume newVolume = new Volume(this.drive, name, offset, length);
                            OnPartitionAdded?.Invoke(this, new AddPartitionEventArgs(newVolume));
                            // Might need some clean up here. Should not rely on the event to add the partition to the database.
                            partitionDatabases[partitionDatabases.Count - 1].RecoverFromJson(partitionElement, recoveredFolder);
                        }
                    }
                }
                else
                {
                    throw new FileLoadException("Database: Drive has no Partition list");
                }
            }
            else
            {
                throw new FileLoadException("Database: Missing Drive object");
            }
        }
    }
}
