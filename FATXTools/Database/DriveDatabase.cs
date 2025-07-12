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

        // Required using: using FATXTools.Recovery;
        public void RecoverFromJson(string path, List<string> recoveredFolder)
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
                        // Check if partition exists, else create & recover
                        if (!LoadRecoveryIfNotExists(partitionElement, recoveredFolder))
                        {
                            var offset = partitionElement.GetProperty("Offset").GetInt64();
                            var length = partitionElement.GetProperty("Length").GetInt64();
                            var name = partitionElement.GetProperty("Name").GetString();

                            // create new volume
                            Volume newVolume = new Volume(this.drive, name, offset, length);

                            OnPartitionAdded?.Invoke(this, new AddPartitionEventArgs(newVolume));

                            // Should be last item now
                            partitionDatabases[partitionDatabases.Count - 1].RecoverFromJson(partitionElement, recoveredFolder);
                        }
                    }
                }
                else
                {
                    throw new FileLoadException("Database: Drive has no Partition list!");
                }
            }
        }


        /// <summary>
        /// Check if the partition is already loaded (by offset, length, or name), otherwise do nothing.
        /// Return true if already present.
        /// </summary>
        /// <summary>
        /// For JSON-based recovery: if partition already exists, call RecoverFromJson; else return false.
        /// Use in DriveDatabase.RecoverFromJson(path, recoveredFolder) workflow.
        /// </summary>
        /// <summary>
        /// Returns true if the partition already exists, else adds it and returns false.
        /// Used for recovery workflows.
        /// </summary>
        private bool LoadRecoveryIfNotExists(JsonElement partitionElement, List<string> recoveredFolder)
        {
            long offset = partitionElement.GetProperty("Offset").GetInt64();

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


        public void LoadRecoverFromJson(string path, List<string> recoveredFolders)
        {
            string json = File.ReadAllText(path);
            var databaseObject = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            if (databaseObject.ContainsKey("Drive"))
            {
                JsonElement driveJsonElement = (JsonElement)databaseObject["Drive"];
                if (driveJsonElement.TryGetProperty("Partitions", out var partitionsElement))
                {
                    foreach (var partitionElement in partitionsElement.EnumerateArray())
                    {
                        if (!LoadRecoveryIfNotExists(partitionElement, recoveredFolders))
                        {
                            var offset = partitionElement.GetProperty("Offset").GetInt64();
                            var length = partitionElement.GetProperty("Length").GetInt64();
                            var name = partitionElement.GetProperty("Name").GetString();
                            Volume newVolume = new Volume(this.drive, name, offset, length);
                            OnPartitionAdded?.Invoke(this, new AddPartitionEventArgs(newVolume));
                            // Should be RecoverFromJson:
                            partitionDatabases[partitionDatabases.Count - 1].RecoverFromJson(partitionElement, recoveredFolders);
                        }
                    }
                }
                else throw new FileLoadException("Database: Drive has no Partition list");
            }
            else throw new FileLoadException("Database: Missing Drive object");
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

    }
}
