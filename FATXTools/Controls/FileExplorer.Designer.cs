namespace FATXTools.Controls
{
    partial class FileExplorer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            splitContainer2 = new System.Windows.Forms.SplitContainer();
            treeView1 = new System.Windows.Forms.TreeView();
            treeContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            runMetadataAnalyzerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            runFileCarverToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            saveSelectedToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            saveAllToolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            listView1 = new System.Windows.Forms.ListView();
            columnHeader8 = new System.Windows.Forms.ColumnHeader();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            columnHeader6 = new System.Windows.Forms.ColumnHeader();
            columnHeader7 = new System.Windows.Forms.ColumnHeader();
            listContextMenu = new System.Windows.Forms.ContextMenuStrip(components);
            runMetadataAnalyzerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            runFileCarverToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            saveSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveAllToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            viewInformationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            treeContextMenu.SuspendLayout();
            listContextMenu.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            splitContainer2.Location = new System.Drawing.Point(0, 0);
            splitContainer2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(treeView1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(listView1);
            splitContainer2.Size = new System.Drawing.Size(1244, 668);
            splitContainer2.SplitterDistance = 208;
            splitContainer2.SplitterWidth = 2;
            splitContainer2.TabIndex = 2;
            // 
            // treeView1
            // 
            treeView1.ContextMenuStrip = treeContextMenu;
            treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            treeView1.HideSelection = false;
            treeView1.Location = new System.Drawing.Point(0, 0);
            treeView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            treeView1.Name = "treeView1";
            treeView1.Size = new System.Drawing.Size(208, 668);
            treeView1.TabIndex = 0;
            treeView1.AfterSelect += treeView1_AfterSelect;
            // 
            // treeContextMenu
            // 
            treeContextMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { runMetadataAnalyzerToolStripMenuItem, runFileCarverToolStripMenuItem, toolStripSeparator2, saveSelectedToolStripMenuItem1, saveAllToolStripMenuItem2 });
            treeContextMenu.Name = "contextMenuStrip1";
            treeContextMenu.Size = new System.Drawing.Size(197, 98);
            // 
            // runMetadataAnalyzerToolStripMenuItem
            // 
            runMetadataAnalyzerToolStripMenuItem.Name = "runMetadataAnalyzerToolStripMenuItem";
            runMetadataAnalyzerToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            runMetadataAnalyzerToolStripMenuItem.Text = "Run Metadata Analyzer";
            runMetadataAnalyzerToolStripMenuItem.Click += runMetadataAnalyzerToolStripMenuItem_Click;
            // 
            // runFileCarverToolStripMenuItem
            // 
            runFileCarverToolStripMenuItem.Name = "runFileCarverToolStripMenuItem";
            runFileCarverToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            runFileCarverToolStripMenuItem.Text = "Run File Carver";
            runFileCarverToolStripMenuItem.Click += runFileCarverToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(193, 6);
            // 
            // saveSelectedToolStripMenuItem1
            // 
            saveSelectedToolStripMenuItem1.Name = "saveSelectedToolStripMenuItem1";
            saveSelectedToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            saveSelectedToolStripMenuItem1.Text = "Save Selected";
            saveSelectedToolStripMenuItem1.Click += treeSaveSelectedToolStripMenuItem1_Click;
            // 
            // saveAllToolStripMenuItem2
            // 
            saveAllToolStripMenuItem2.Name = "saveAllToolStripMenuItem2";
            saveAllToolStripMenuItem2.Size = new System.Drawing.Size(196, 22);
            saveAllToolStripMenuItem2.Text = "Save All";
            saveAllToolStripMenuItem2.Click += saveAllToolStripMenuItem2_Click;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader8, columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5, columnHeader6, columnHeader7 });
            listView1.ContextMenuStrip = listContextMenu;
            listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            listView1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.857143F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            listView1.FullRowSelect = true;
            listView1.Location = new System.Drawing.Point(0, 0);
            listView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(1034, 668);
            listView1.TabIndex = 0;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.Details;
            listView1.ColumnClick += listView1_ColumnClick;
            listView1.DoubleClick += listView1_DoubleClick;
            listView1.KeyDown += listView1_KeyDown;
            // 
            // columnHeader8
            // 
            columnHeader8.Text = "";
            columnHeader8.Width = 44;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Name";
            columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Size";
            columnHeader2.Width = 150;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Date Created";
            columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Date Modified";
            columnHeader4.Width = 150;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Date Accessed";
            columnHeader5.Width = 150;
            // 
            // columnHeader6
            // 
            columnHeader6.Text = "Offset";
            columnHeader6.Width = 150;
            // 
            // columnHeader7
            // 
            columnHeader7.Text = "Cluster";
            columnHeader7.Width = 150;
            // 
            // listContextMenu
            // 
            listContextMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            listContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { runMetadataAnalyzerToolStripMenuItem1, runFileCarverToolStripMenuItem1, toolStripSeparator1, saveSelectedToolStripMenuItem, saveAllToolStripMenuItem, saveAllToolStripMenuItem1, toolStripSeparator3, viewInformationToolStripMenuItem });
            listContextMenu.Name = "listContextMenu";
            listContextMenu.Size = new System.Drawing.Size(197, 148);
            // 
            // runMetadataAnalyzerToolStripMenuItem1
            // 
            runMetadataAnalyzerToolStripMenuItem1.Name = "runMetadataAnalyzerToolStripMenuItem1";
            runMetadataAnalyzerToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            runMetadataAnalyzerToolStripMenuItem1.Text = "Run Metadata Analyzer";
            runMetadataAnalyzerToolStripMenuItem1.Click += runMetadataAnalyzerToolStripMenuItem_Click;
            // 
            // runFileCarverToolStripMenuItem1
            // 
            runFileCarverToolStripMenuItem1.Name = "runFileCarverToolStripMenuItem1";
            runFileCarverToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            runFileCarverToolStripMenuItem1.Text = "Run File Carver";
            runFileCarverToolStripMenuItem1.Click += runFileCarverToolStripMenuItem_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(193, 6);
            // 
            // saveSelectedToolStripMenuItem
            // 
            saveSelectedToolStripMenuItem.Name = "saveSelectedToolStripMenuItem";
            saveSelectedToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            saveSelectedToolStripMenuItem.Text = "Save Selected";
            saveSelectedToolStripMenuItem.Click += listSaveSelectedToolStripMenuItem_Click;
            // 
            // saveAllToolStripMenuItem
            // 
            saveAllToolStripMenuItem.Name = "saveAllToolStripMenuItem";
            saveAllToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            saveAllToolStripMenuItem.Text = "Save Current Directory";
            saveAllToolStripMenuItem.Click += saveAllToolStripMenuItem_Click;
            // 
            // saveAllToolStripMenuItem1
            // 
            saveAllToolStripMenuItem1.Name = "saveAllToolStripMenuItem1";
            saveAllToolStripMenuItem1.Size = new System.Drawing.Size(196, 22);
            saveAllToolStripMenuItem1.Text = "Save All";
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(193, 6);
            // 
            // viewInformationToolStripMenuItem
            // 
            viewInformationToolStripMenuItem.Name = "viewInformationToolStripMenuItem";
            viewInformationToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            viewInformationToolStripMenuItem.Text = "View Information";
            viewInformationToolStripMenuItem.Click += viewInformationToolStripMenuItem_Click;
            // 
            // FileExplorer
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            Controls.Add(splitContainer2);
            Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            Name = "FileExplorer";
            Size = new System.Drawing.Size(1244, 668);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            treeContextMenu.ResumeLayout(false);
            listContextMenu.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem runMetadataAnalyzerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem runFileCarverToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip listContextMenu;
        private System.Windows.Forms.ToolStripMenuItem runMetadataAnalyzerToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem runFileCarverToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveSelectedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveSelectedToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveAllToolStripMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem viewInformationToolStripMenuItem;
    }
}
