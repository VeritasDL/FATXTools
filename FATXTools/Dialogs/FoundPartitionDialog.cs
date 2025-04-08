using FATX;
using FATX.FileSystem;
using FATXTools.Forms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FATXTools.Dialogs
{
    public partial class FoundPartitionDialog : Form
    {
        public static readonly DialogResult DialogResultLoadPart = DialogResult.OK;
        public List<(string Name, long Offset, long Length)> SelectedPartitions { get; private set; } = new();
        public FoundPartitionDialog(List<(string Name, long Offset, long Length)> partitions)
        {
            InitializeComponent();
            listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Name", 150);
            listView1.Columns.Add("Offset", 200);
            listView1.Columns.Add("Length", 200);
            var sorted = partitions.OrderBy(p => p.Offset);
            foreach (var (name, offset, length) in sorted)
            {
                var item = new ListViewItem(name)
                {
                    Tag = (name, offset, length),
                    Checked = false // Set to true if you want items selected by default
                };

                item.SubItems.Add($"0x{offset:X}");
                item.SubItems.Add($"0x{length:X}");
                listView1.Items.Add(item);
            }
            button1.Click += (_, _) => DialogResult = DialogResult.OK;
            button2.Click += (_, _) => DialogResult = DialogResult.Cancel;
            selectAllBox.CheckedChanged += (s, e) =>
            {
                foreach (ListViewItem item in listView1.Items)
                {
                    item.Checked = selectAllBox.Checked;
                }
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SelectedPartitions.Clear();
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Checked && item.Tag is ValueTuple<string, long, long> part)
                {
                    SelectedPartitions.Add(part);
                }
            }

            // Do NOT warn about empty selection — allow user to confirm no additions
            this.DialogResult = DialogResult.OK;
            this.Close();
        }


    }
}