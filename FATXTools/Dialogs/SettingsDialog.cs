using FATX.Analyzers;
using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace FATXTools.Dialogs
{
    public partial class SettingsDialog : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public FileCarverInterval FileCarverInterval
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LogFile
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DriveWrite
        {
            get;
            set;
        }

        public SettingsDialog()
        {
            InitializeComponent();

            this.FileCarverInterval = Properties.Settings.Default.FileCarverInterval;
            this.LogFile = Properties.Settings.Default.LogFile;
            this.DriveWrite = Properties.Settings.Default.DriveWrite;
            switch (this.FileCarverInterval)
            {
                case FileCarverInterval.Byte:
                    comboBox1.SelectedIndex = 0;
                    break;
                case FileCarverInterval.Align:
                    comboBox1.SelectedIndex = 1;
                    break;
                case FileCarverInterval.Sector:
                    comboBox1.SelectedIndex = 2;
                    break;
                case FileCarverInterval.Page:
                    comboBox1.SelectedIndex = 3;
                    break;
                case FileCarverInterval.Cluster:
                    comboBox1.SelectedIndex = 4;
                    break;
            }

            textBox1.Text = this.LogFile;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: FileCarverInterval = FileCarverInterval.Byte; break;
                case 1: FileCarverInterval = FileCarverInterval.Align; break;
                case 2: FileCarverInterval = FileCarverInterval.Sector; break;
                case 3: FileCarverInterval = FileCarverInterval.Page; break;
                case 4: FileCarverInterval = FileCarverInterval.Cluster; break;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LogFile = textBox1.Text;
        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                DriveWrite = true;
            }
            else
            {
                DriveWrite = false;
            }
        }

    }
}
