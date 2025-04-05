using FATX;
using FATX.FileSystem;
using FATXTools.Forms;
using System;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace FATXTools.Dialogs
{
    public partial class FoundPartitionDialog : Form
    {
        public static readonly DialogResult DialogResultLoadPart = DialogResult.OK;

        public FoundPartitionDialog()
        {
            InitializeComponent();

        }
    }
}