namespace FATXTools.Forms
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            label1 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openDeviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            historyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            loadFromJSONToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            settingsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            driveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            searchForPartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            devKitHeadderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dev1746toolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            dev1838ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            retail1888ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            retail2125618ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            managePartitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addPartitionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 24);
            splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.DragDrop += MainWindow_DragDrop;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(textBox1);
            splitContainer1.Size = new System.Drawing.Size(1346, 613);
            splitContainer1.SplitterDistance = 399;
            splitContainer1.SplitterWidth = 2;
            splitContainer1.TabIndex = 4;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(2, 6);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(30, 15);
            label1.TabIndex = 3;
            label1.Text = "Log:";
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Font = new System.Drawing.Font("Consolas", 8.142858F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            textBox1.Location = new System.Drawing.Point(5, 22);
            textBox1.Margin = new System.Windows.Forms.Padding(2);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBox1.Size = new System.Drawing.Size(1337, 168);
            textBox1.TabIndex = 2;
            textBox1.WordWrap = false;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            statusStrip1.Location = new System.Drawing.Point(0, 615);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            statusStrip1.Size = new System.Drawing.Size(1346, 22);
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // backgroundWorker1
            // 
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            // 
            // backgroundWorker2
            // 
            backgroundWorker2.WorkerReportsProgress = true;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { openImageToolStripMenuItem, openDeviceToolStripMenuItem, historyToolStripMenuItem, toolStripSeparator3, saveToolStripMenuItem, loadToolStripMenuItem, toolStripSeparator2, settingsToolStripMenuItem1, toolStripSeparator1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 22);
            fileToolStripMenuItem.Text = "File";
            // 
            // openImageToolStripMenuItem
            // 
            openImageToolStripMenuItem.Name = "openImageToolStripMenuItem";
            openImageToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            openImageToolStripMenuItem.Text = "Open Image";
            openImageToolStripMenuItem.Click += openImageToolStripMenuItem_Click;
            // 
            // openDeviceToolStripMenuItem
            // 
            openDeviceToolStripMenuItem.Name = "openDeviceToolStripMenuItem";
            openDeviceToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            openDeviceToolStripMenuItem.Text = "Open Device";
            openDeviceToolStripMenuItem.Click += openDeviceToolStripMenuItem_Click;
            // 
            // historyToolStripMenuItem
            // 
            historyToolStripMenuItem.Name = "historyToolStripMenuItem";
            historyToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            historyToolStripMenuItem.Text = "History";
            historyToolStripMenuItem.Click += openRecentFileStripMenuItem_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(138, 6);
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { saveToJSONToolStripMenuItem });
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            saveToolStripMenuItem.Text = "Save";
            // 
            // saveToJSONToolStripMenuItem
            // 
            saveToJSONToolStripMenuItem.Name = "saveToJSONToolStripMenuItem";
            saveToJSONToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            saveToJSONToolStripMenuItem.Text = "Save To JSON";
            saveToJSONToolStripMenuItem.Click += saveToJSONToolStripMenuItem_Click;
            // 
            // loadToolStripMenuItem
            // 
            loadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { loadFromJSONToolStripMenuItem });
            loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            loadToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            loadToolStripMenuItem.Text = "Load";
            // 
            // loadFromJSONToolStripMenuItem
            // 
            loadFromJSONToolStripMenuItem.Name = "loadFromJSONToolStripMenuItem";
            loadFromJSONToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            loadFromJSONToolStripMenuItem.Text = "Load From JSON";
            loadFromJSONToolStripMenuItem.Click += loadFromJSONToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(138, 6);
            // 
            // settingsToolStripMenuItem1
            // 
            settingsToolStripMenuItem1.Name = "settingsToolStripMenuItem1";
            settingsToolStripMenuItem1.Size = new System.Drawing.Size(141, 22);
            settingsToolStripMenuItem1.Text = "Settings";
            settingsToolStripMenuItem1.Click += settingsToolStripMenuItem1_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(138, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new System.Drawing.Size(141, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 22);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new System.Drawing.Size(113, 22);
            aboutToolStripMenuItem.Text = "About..";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { fileToolStripMenuItem, driveToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            menuStrip1.Size = new System.Drawing.Size(1346, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // driveToolStripMenuItem
            // 
            driveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { searchForPartitionsToolStripMenuItem, managePartitionsToolStripMenuItem, addPartitionToolStripMenuItem });
            driveToolStripMenuItem.Name = "driveToolStripMenuItem";
            driveToolStripMenuItem.Size = new System.Drawing.Size(46, 22);
            driveToolStripMenuItem.Text = "Drive";
            // 
            // searchForPartitionsToolStripMenuItem
            // 
            searchForPartitionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { devKitHeadderToolStripMenuItem, dev1746toolStripMenuItem, dev1838ToolStripMenuItem, retail1888ToolStripMenuItem, retail2125618ToolStripMenuItem });
            searchForPartitionsToolStripMenuItem.Name = "searchForPartitionsToolStripMenuItem";
            searchForPartitionsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            searchForPartitionsToolStripMenuItem.Text = "Search For Partitions";
            // 
            // devKitHeadderToolStripMenuItem
            // 
            devKitHeadderToolStripMenuItem.Name = "devKitHeadderToolStripMenuItem";
            devKitHeadderToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            devKitHeadderToolStripMenuItem.Text = "DevKit Headder";
            devKitHeadderToolStripMenuItem.Click += devKitHeadderToolStripMenuItem_Click;
            // 
            // dev1746toolStripMenuItem
            // 
            dev1746toolStripMenuItem.Name = "dev1746toolStripMenuItem";
            dev1746toolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            dev1746toolStripMenuItem.Text = "Dev [1746]";
            //dev1746toolStripMenuItem.Click += dev1746toolStripMenuItem_Click;
            // 
            // dev1838ToolStripMenuItem
            // 
            dev1838ToolStripMenuItem.Name = "dev1838ToolStripMenuItem";
            dev1838ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            dev1838ToolStripMenuItem.Text = "Dev [1838]";
            //dev1838ToolStripMenuItem.Click += dev1838ToolStripMenuItem_Click;
            // 
            // retail1888ToolStripMenuItem
            // 
            retail1888ToolStripMenuItem.Name = "retail1888ToolStripMenuItem";
            retail1888ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            retail1888ToolStripMenuItem.Text = "Retail [1888]";
            //retail1888ToolStripMenuItem.Click += retail1888ToolStripMenuItem_Click;
            // 
            // retail2125618ToolStripMenuItem
            // 
            retail2125618ToolStripMenuItem.Name = "retail2125618ToolStripMenuItem";
            retail2125618ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            retail2125618ToolStripMenuItem.Text = "Retail [21256.18]";
            //retail2125618ToolStripMenuItem.Click += retail2125618ToolStripMenuItem_Click;
            // 
            // managePartitionsToolStripMenuItem
            // 
            managePartitionsToolStripMenuItem.Enabled = false;
            managePartitionsToolStripMenuItem.Name = "managePartitionsToolStripMenuItem";
            managePartitionsToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            managePartitionsToolStripMenuItem.Text = "Manage Partitions";
            managePartitionsToolStripMenuItem.Click += managePartitionsToolStripMenuItem_Click;
            // 
            // addPartitionToolStripMenuItem
            // 
            addPartitionToolStripMenuItem.Name = "addPartitionToolStripMenuItem";
            addPartitionToolStripMenuItem.Size = new System.Drawing.Size(182, 22);
            addPartitionToolStripMenuItem.Text = "Add Partition";
            addPartitionToolStripMenuItem.Click += addPartitionToolStripMenuItem_Click;
            // 
            // MainWindow
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1346, 637);
            Controls.Add(statusStrip1);
            Controls.Add(splitContainer1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new System.Windows.Forms.Padding(2);
            Name = "MainWindow";
            Text = "FATX-Recover";
            FormClosing += MainWindow_FormClosing;
            DragDrop += MainWindow_DragDrop;
            DragEnter += MainWindow_DragEnter;
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDeviceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem driveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchForPartitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem managePartitionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addPartitionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFromJSONToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem historyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem devKitHeadderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dev1746toolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dev1838ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retail1888ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem retail2125618ToolStripMenuItem;
    }
}

