﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using FATX.Drive;
using FATX.FileSystem;
using FATX.Streams;

namespace FATXTools.Dialogs
{
    public partial class PartitionManagerDialog : Form
    {
        private List<Volume> _volumes;
        private XDrive _drive;

        public PartitionManagerDialog()
        {
            InitializeComponent();
        }

        public PartitionManagerDialog(XDrive reader, List<Volume> volumes)
        {
            InitializeComponent();

            this._drive = reader;
            this._volumes = volumes;

            PopulateList(volumes);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NewPartitionDialog dialog = new NewPartitionDialog();
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                var volume = new Volume(
                    new SubStream(_drive.Stream, dialog.PartitionOffset, dialog.PartitionLength),
                    _drive is XboxDrive ? Platform.Xbox : Platform.X360,
                    dialog.PartitionName,
                    dialog.PartitionOffset,
                    dialog.PartitionLength
                );

                _volumes.Add(volume);

                PopulateList(_volumes);
            }
        }

        private void PopulateList(List<Volume> volumes)
        {
            listView1.Items.Clear();

            foreach (var volume in volumes)
            {
                ListViewItem item = new ListViewItem(volume.Name);
                item.SubItems.Add("0x" + volume.Offset.ToString("X"));
                item.SubItems.Add("0x" + volume.Length.ToString("X"));

                listView1.Items.Add(item);
            }
        }
    }
}
