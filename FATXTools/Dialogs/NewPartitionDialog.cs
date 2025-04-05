using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FATXTools.Dialogs
{
    public partial class NewPartitionDialog : Form
    {
        public static readonly DialogResult DialogResultLoadArray = DialogResult.Continue;
        public (string PartitionName, long PartitionOffset, long PartitionLength)[] Partitions
        {
            get
            {
                var partitions = new List<(string PartitionName, long PartitionOffset, long PartitionLength)>();
                var lines = textBox1.Text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { '(', ')', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        var name = parts[0].Trim('"');
                        var offset = ParseHexInput(parts[1]);
                        var length = ParseHexInput(parts[2]);
                        partitions.Add((name, offset, length));
                    }
                }
                return partitions.ToArray();
            }
        }
        public NewPartitionDialog()
        {
            InitializeComponent();
        }
        public string PartitionName
        {
            get => textBox1.Text;
        }

        public long PartitionOffset
        {
            get => ParseHexInput(textBox2.Text);
        }
        public long PartitionLength
        {
            get => ParseHexInput(textBox3.Text);
        }

        public static long ParseHexInput(string input)
        {
            input = input.Replace("(", "").Replace(")", "").Replace(",", "").Replace(" ", "");
            if (input == "")
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                input = input.Substring(2);
            }
            return long.Parse(input, System.Globalization.NumberStyles.HexNumber);
        }

        private void LoadPartitionArray(object sender, EventArgs e)
        {
        }
    }
}
