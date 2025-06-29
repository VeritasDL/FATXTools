﻿using FATX;
using FATX.FileSystem;
using FATXTools.Controls;
using FATXTools.Database;
using FATXTools.DiskTypes;
using FATXTools.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FATXTools
{
    public partial class DriveView : UserControl
    {
        /// <summary>
        /// List of loaded drives.
        /// </summary>
        //private List<DriveReader> driveList = new List<DriveReader>();

        /// <summary>
        /// Currently loaded drive.
        /// </summary>
        private DriveReader drive;

        private string driveName;

        /// <summary>
        /// List of partitions in this drive.
        /// </summary>
        private List<PartitionView> partitionViews = new List<PartitionView>();

        private TaskRunner taskRunner;

        public event EventHandler TaskStarted;

        public event EventHandler TaskCompleted;

        public event EventHandler<PartitionSelectedEventArgs> TabSelectionChanged;

        private DriveDatabase driveDatabase;

        public DriveView()
        {
            InitializeComponent();
        }

        public void AddDrive(string name, DriveReader drive)
        {
            this.driveName = name;
            this.drive = drive;

            this.driveDatabase = new DriveDatabase(name, drive);
            this.driveDatabase.OnPartitionAdded += DriveDatabase_OnPartitionAdded;
            this.driveDatabase.OnPartitionRemoved += DriveDatabase_OnPartitionRemoved;

            // Single task runner for this drive
            // Currently only one task will be allowed to operate on a drive to avoid race conditions.
            this.taskRunner = new TaskRunner(this.ParentForm);
            this.taskRunner.TaskStarted += TaskRunner_TaskStarted;
            this.taskRunner.TaskCompleted += TaskRunner_TaskCompleted;

            this.partitionTabControl.MouseClick += PartitionTabControl_MouseClick;

            foreach (var volume in drive.Partitions)
            {
                AddPartition(volume);
            }

            // Fire SelectedIndexChanged event.
            SelectedIndexChanged();
        }



        private void DriveDatabase_OnPartitionRemoved(object sender, RemovePartitionEventArgs e)
        {
            var index = e.Index;
            partitionTabControl.TabPages.RemoveAt(index);
            partitionViews.RemoveAt(index);
        }

        private void PartitionTabControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (var i = 0; i < partitionTabControl.TabCount; i++)
                {
                    Rectangle r = partitionTabControl.GetTabRect(i);
                    if (r.Contains(e.Location))
                    {
                        partitionTabControl.SelectedIndex = i;
                        this.contextMenuStrip.Show(this.partitionTabControl, e.Location);
                        break;
                    }
                }
            }
        }

        private void DriveDatabase_OnPartitionAdded(object sender, AddPartitionEventArgs e)
        {
            AddPartition(e.Volume);
        }

        private void TaskRunner_TaskCompleted(object sender, EventArgs e)
        {
            TaskCompleted?.Invoke(sender, e);
        }

        private void TaskRunner_TaskStarted(object sender, EventArgs e)
        {
            TaskStarted?.Invoke(sender, e);
        }

        public void AddPartition(Volume volume)
        {
            try
            {
                volume.Mount();

                Console.WriteLine($"Successfully mounted {volume.Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to mount {volume.Name}: {e.Message}");
            }

            var page = new TabPage(volume.Name);
            var partitionDatabase = driveDatabase.AddPartition(volume);
            var partitionView = new PartitionView(taskRunner, volume, partitionDatabase);
            partitionView.Dock = DockStyle.Fill;
            page.Controls.Add(partitionView);
            partitionTabControl.TabPages.Add(page);
            partitionViews.Add(partitionView);
        }

        public DriveReader GetDrive()
        {
            return drive;
        }

        public List<Volume> GetVolumes()
        {
            return partitionViews.Select(partitionView => partitionView.Volume).ToList();
        }

        public void Save(string path)
        {
            driveDatabase.Save(path);
        }

        public void LoadFromJson(string path)
        {
            driveDatabase.LoadFromJson(path);
        }

        public void RecoverFromJson(string jsonPath, string recoveredFolder)
        {
            // Assume driveDatabase is a member field
            if (this.driveDatabase != null)
            {
                this.driveDatabase.RecoverFromJson(jsonPath, recoveredFolder);
            }
        }

        private void SelectedIndexChanged()
        {
            TabSelectionChanged?.Invoke(this, partitionTabControl.TabCount == 0 ? null : new PartitionSelectedEventArgs()
            {
                volume = partitionViews[partitionTabControl.SelectedIndex].Volume
            });
        }

        private void partitionTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedIndexChanged();
        }

        private void ToolStripMenuItem1_Click(object sender, System.EventArgs e)
        {
            var dialogResult = MessageBox.Show("Are you sure you want to remove this partition?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                driveDatabase.RemovePartition(partitionTabControl.SelectedIndex);
            }
        }
    }
}
