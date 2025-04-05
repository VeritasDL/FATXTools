﻿using FATX.FileSystem;
using FATXTools.Controls;
using FATXTools.Dialogs;
using FATXTools.DiskTypes;
using FATXTools.Utilities;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FATXTools.Forms
{
    public partial class MainWindow : Form
    {
        private DriveView driveView;
        private List<string> fileHistory = new List<string>();
        private const int MaxHistoryCount = 30;
        private const string HistoryFilePath = "FATxToolHistory.txt";
        private const string ApplicationTitle = "FATX-Recover";

        public MainWindow()
        {
            InitializeComponent();

            this.Text = ApplicationTitle;
            this.Name = "MainWindow";
            DisableDatabaseOptions();

            Console.SetOut(new LogWriter(this.textBox1));
            Console.WriteLine("--------------------------------");
            Console.WriteLine("FATX-Tools v0.3");
            Console.WriteLine("--------------------------------");
            if (File.Exists("FATxToolHistory.txt"))
            {
                LoadFileHistory();
            }
            else
            {
                File.Create("FATxToolHistory.txt").Dispose();
                LoadFileHistory();
                UpdateFileHistoryMenu();
            }
        }

        public class LogWriter : TextWriter
        {
            private TextBox textBox;
            private delegate void SafeCallDelegate(string text);
            public LogWriter(TextBox textBox)
            {
                this.textBox = textBox;
            }

            public override void Write(char value)
            {
                textBox.Text += value;
            }

            public override void Write(string value)
            {
                textBox.AppendText(value);
            }

            public override void WriteLine()
            {
                textBox.AppendText(NewLine);
            }

            public override void WriteLine(string value)
            {
                if (textBox.InvokeRequired)
                {
                    var d = new SafeCallDelegate(WriteLine);
                    textBox.BeginInvoke(d, new object[] { value });
                }
                else
                {
                    textBox.AppendText(value + NewLine);
                }
            }

            public override Encoding Encoding
            {
                get { return Encoding.ASCII; }
            }
        }

        private void CreateNewDriveView(string path)
        {
            this.Text = $"{ApplicationTitle} - {Path.GetFileName(path)}";

            // Destroy the current drive view
            splitContainer1.Panel1.Controls.Remove(driveView);

            // Create a new view for this drive
            driveView = new DriveView();
            driveView.Dock = DockStyle.Fill;
            driveView.TabSelectionChanged += DriveView_TabSelectionChanged;
            driveView.TaskStarted += DriveView_TaskStarted;
            driveView.TaskCompleted += DriveView_TaskCompleted;

            // Add the view to the panel
            splitContainer1.Panel1.Controls.Add(driveView);
        }

        private void DriveView_TaskCompleted(object sender, EventArgs e)
        {
            EnableOpenOptions();
            EnableDatabaseOptions();
        }

        private void DriveView_TaskStarted(object sender, EventArgs e)
        {
            DisableOpenOptions();
            DisableDatabaseOptions();
        }

        private void DriveView_TabSelectionChanged(object sender, PartitionSelectedEventArgs e)
        {
            if (e == null)
            {
                statusStrip1.Items.Clear();
            }
            else
            {
                var volume = e.volume;

                statusStrip1.Items.Clear();
                if (volume.Mounted)
                {
                    var usedSpace = volume.GetUsedSpace();
                    var freeSpace = volume.GetFreeSpace();
                    var totalSpace = volume.GetTotalSpace();

                    statusStrip1.Items.Add($"Volume Offset: 0x{volume.Offset:X}");
                    statusStrip1.Items.Add($"Volume Length: 0x{volume.Length:X}");
                    statusStrip1.Items.Add($"Used Space: {Utility.FormatBytes(usedSpace)}");
                    statusStrip1.Items.Add($"Free Space: {Utility.FormatBytes(freeSpace)}");
                    statusStrip1.Items.Add($"Total Space: {Utility.FormatBytes(totalSpace)}");
                }
            }
        }

        private void EnableDatabaseOptions()
        {
            loadToolStripMenuItem.Enabled = true;
            saveToolStripMenuItem.Enabled = true;

            addPartitionToolStripMenuItem.Enabled = true;
            searchForPartitionsToolStripMenuItem.Enabled = true;
            managePartitionsToolStripMenuItem.Enabled = true;
        }

        private void DisableDatabaseOptions()
        {
            loadToolStripMenuItem.Enabled = false;
            saveToolStripMenuItem.Enabled = false;

            addPartitionToolStripMenuItem.Enabled = false;
            searchForPartitionsToolStripMenuItem.Enabled = false;
            managePartitionsToolStripMenuItem.Enabled = false;
        }

        private void EnableOpenOptions()
        {
            openImageToolStripMenuItem.Enabled = true;
            openDeviceToolStripMenuItem.Enabled = true;
            searchForPartitionsToolStripMenuItem.Enabled = true;
            managePartitionsToolStripMenuItem.Enabled = true;
        }

        private void DisableOpenOptions()
        {
            openImageToolStripMenuItem.Enabled = false;
            openDeviceToolStripMenuItem.Enabled = false;
        }

        private void OpenDiskImage(string path)
        {
            CreateNewDriveView(path);

            string fileName = Path.GetFileName(path);

            RawImage rawImage = new RawImage(path);
            driveView.AddDrive(fileName, rawImage);
            AddToFileHistory(path);
            EnableDatabaseOptions();
        }

        private void OpenDisk(string device)
        {
            CreateNewDriveView(device);

            SafeFileHandle handle = WinApi.CreateFile(device,
                       FileAccess.Read,
                       FileShare.None,
                       IntPtr.Zero,
                       FileMode.Open,
                       0,
                       IntPtr.Zero);
            long length = WinApi.GetDiskCapactity(handle);
            long sectorLength = WinApi.GetSectorSize(handle);
            PhysicalDisk drive = new PhysicalDisk(handle, length, sectorLength);
            driveView.AddDrive(device, drive);
            AddToFileHistory(device);
            EnableDatabaseOptions();
        }

        private void openImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                OpenDiskImage(ofd.FileName);

            }
        }

        private void openDeviceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            if (!isAdmin)
            {
                MessageBox.Show("You must re-run this program with Administrator privileges\n" +
                                "in order to read from physical drives.",
                                "Cannot perform operation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DeviceSelectionDialog ds = new DeviceSelectionDialog();
            if (ds.ShowDialog() == DialogResult.OK)
            {
                OpenDisk(ds.SelectedDevice);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Developed by aerosoul94\n" +
                "Source code: https://github.com/aerosoul94/FATXTools\n" +
                "Please report any bugs\n",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Only 1 file is allowed.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 1)
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    e.Effect = DragDropEffects.Link;
                }
            }
            else
            {
                // Not a file.
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainWindow_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 1)
            {
                MessageBox.Show("You may only drop one file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string path = files[0];
                OpenDiskImage(path);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void managePartitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (driveView != null)
            {
                //PartitionManagerForm partitionManagerForm = new PartitionManagerForm(driveView.GetDrive(), driveView.GetDrive().GetPartitions());
                //partitionManagerForm.ShowDialog();
            }
        }

        private void devKitHeadderToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void retail1888ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void retail2125618ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void addPartitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewPartitionDialog partitionDialog = new NewPartitionDialog();
            var dialogResult = partitionDialog.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                string arrayInput = partitionDialog.textBox4.Text;

                if (!string.IsNullOrWhiteSpace(arrayInput))
                {
                    var lines = arrayInput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string line in lines)
                    {
                        var match = Regex.Match(line, @"\(\s*""(?<name>[^""]+)""\s*,\s*0x(?<offset>[0-9A-Fa-f]+)\s*,\s*0x(?<length>[0-9A-Fa-f]+)\s*\)");
                        if (!match.Success)
                            continue;

                        string name = match.Groups["name"].Value;
                        long offset = Convert.ToInt64(match.Groups["offset"].Value, 16);
                        long length = Convert.ToInt64(match.Groups["length"].Value, 16);

                        driveView.AddPartition(new Volume(driveView.GetDrive(), name, offset, length));
                    }
                }
                else
                {
                    driveView.AddPartition(new Volume(driveView.GetDrive(),
                        partitionDialog.PartitionName,
                        partitionDialog.PartitionOffset,
                        partitionDialog.PartitionLength));
                }
            }
        }

        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SettingsDialog settings = new SettingsDialog();
            if (settings.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.FileCarverInterval = settings.FileCarverInterval;
                Properties.Settings.Default.LogFile = settings.LogFile;

                Properties.Settings.Default.Save();
            }
        }

        private void saveToJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "JSON File (*.json)|*.json"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                driveView.Save(saveFileDialog.FileName);

                Console.WriteLine($"Finished saving database: {saveFileDialog.FileName}");
            }
        }

        private void loadFromJSONToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "JSON File (*.json)|*.json"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var dialogResult = MessageBox.Show($"Loading a database will overwrite current analysis progress.\n"
                    + $"Are you sure you want to load \'{Path.GetFileName(openFileDialog.FileName)}\'?",
                    "Load File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    driveView.LoadFromJson(openFileDialog.FileName);

                    Console.WriteLine($"Finished loading database: {openFileDialog.FileName}");
                }
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO: For any partition, if any analysis was made, then we should ask.
            // TODO: Add setting for auto-saving (maybe at run-time or while closing)
            if (driveView != null)
            {
                var dialogResult = MessageBox.Show("Would you like to save progress before closing?", "Save Progress", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog()
                    {
                        Filter = "JSON File (*.json)|*.json"
                    };

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        driveView.Save(saveFileDialog.FileName);

                        Console.WriteLine($"Finished saving database: {saveFileDialog.FileName}");
                    }
                    else
                    {
                        // User may have accidentally cancelled? Maybe try again?
                    }
                }
                else if (dialogResult == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // TODO: handle closing dialogs
            e.Cancel = false;
        }
        private void openRecentFileStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                string filePath = item.Text;
                OpenDiskImage(filePath);
            }
        }
        private void AddToFileHistory(string device)
        {
            if (fileHistory.Contains(device))
                fileHistory.Remove(device);

            fileHistory.Insert(0, device);

            if (fileHistory.Count > MaxHistoryCount)
                fileHistory.RemoveAt(fileHistory.Count - 1);

            SaveFileHistory();
            UpdateFileHistoryMenu();
        }

        private void LoadFileHistory()
        {
            if (File.Exists(HistoryFilePath))
                fileHistory = File.ReadAllLines(HistoryFilePath).ToList();
            else
                File.Create(HistoryFilePath).Dispose();

            UpdateFileHistoryMenu();
        }

        private void SaveFileHistory()
        {
            File.WriteAllLines(HistoryFilePath, fileHistory);
        }

        private void UpdateFileHistoryMenu()
        {
            historyToolStripMenuItem.DropDownItems.Clear();

            if (fileHistory.Count == 0)
            {
                var emptyItem = new ToolStripMenuItem("No history found");
                emptyItem.Enabled = false;
                historyToolStripMenuItem.DropDownItems.Add(emptyItem);
                return;
            }

            foreach (var file in fileHistory)
            {
                var item = new ToolStripMenuItem(file);
                item.Click += openRecentFileStripMenuItem_Click;
                historyToolStripMenuItem.DropDownItems.Add(item);
            }
        }
    }
}
