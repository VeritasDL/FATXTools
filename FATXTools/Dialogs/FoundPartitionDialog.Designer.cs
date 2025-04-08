using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FATXTools.Dialogs
{
    partial class FoundPartitionDialog
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
        /// 
        public void InitializeComponent()
        {
            button1 = new Button();
            button2 = new Button();
            listView1 = new ListView();
            label1 = new Label();
            selectAllBox = new CheckBox();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(30, 415);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Ok";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(701, 415);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(75, 23);
            button2.TabIndex = 1;
            button2.Text = "Close";
            button2.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.CheckBoxes = true;
            listView1.FullRowSelect = true;
            listView1.Location = new System.Drawing.Point(12, 51);
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(764, 338);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = View.Details;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(384, 20);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(94, 15);
            label1.TabIndex = 3;
            label1.Text = "Found Partitions";
            // 
            // checkBox1
            // 
            selectAllBox.AutoSize = true;
            selectAllBox.Location = new System.Drawing.Point(12, 20);
            selectAllBox.Name = "checkBox1";
            selectAllBox.Size = new System.Drawing.Size(74, 19);
            selectAllBox.TabIndex = 4;
            selectAllBox.Text = "Select All";
            selectAllBox.UseVisualStyleBackColor = true;
            // 
            // FoundPartitionDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 450);
            Controls.Add(selectAllBox);
            Controls.Add(label1);
            Controls.Add(listView1);
            Controls.Add(button2);
            Controls.Add(button1);
            Name = "FoundPartitionDialog";
            Text = "Search For Partitions";
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label1;
        private CheckBox selectAllBox;
    }
}