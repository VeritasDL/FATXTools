namespace FATXTools.Dialogs
{
    partial class NewPartitionDialog
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
        public void InitializeComponent()
        {
            label1 = new System.Windows.Forms.Label();
            textBox1 = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            textBox2 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            textBox3 = new System.Windows.Forms.TextBox();
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            textBox4 = new System.Windows.Forms.TextBox();
            button3 = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 21);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(39, 15);
            label1.TabIndex = 0;
            label1.Text = "Name";
            // 
            // textBox1
            // 
            textBox1.Location = new System.Drawing.Point(7, 37);
            textBox1.Margin = new System.Windows.Forms.Padding(2);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(264, 23);
            textBox1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(7, 69);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(39, 15);
            label2.TabIndex = 2;
            label2.Text = "Offset";
            // 
            // textBox2
            // 
            textBox2.Location = new System.Drawing.Point(7, 87);
            textBox2.Margin = new System.Windows.Forms.Padding(2);
            textBox2.Name = "textBox2";
            textBox2.Size = new System.Drawing.Size(264, 23);
            textBox2.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 119);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(44, 15);
            label3.TabIndex = 4;
            label3.Text = "Length";
            // 
            // textBox3
            // 
            textBox3.Location = new System.Drawing.Point(10, 136);
            textBox3.Margin = new System.Windows.Forms.Padding(2);
            textBox3.Name = "textBox3";
            textBox3.Size = new System.Drawing.Size(261, 23);
            textBox3.TabIndex = 5;
            // 
            // button1
            // 
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.Location = new System.Drawing.Point(11, 169);
            button1.Margin = new System.Windows.Forms.Padding(2);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(117, 34);
            button1.TabIndex = 6;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(152, 169);
            button2.Margin = new System.Windows.Forms.Padding(2);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(117, 34);
            button2.TabIndex = 7;
            button2.Text = "Cancel";
            button2.UseVisualStyleBackColor = true;
            // 
            // textBox4
            // 
            textBox4.Location = new System.Drawing.Point(307, 37);
            textBox4.Margin = new System.Windows.Forms.Padding(2);
            textBox4.Multiline = true;
            textBox4.Name = "textBox4";
            textBox4.Size = new System.Drawing.Size(539, 122);
            textBox4.TabIndex = 8;
            // 
            // button3
            // 
            button3.DialogResult = System.Windows.Forms.DialogResult.OK;
            button3.Location = new System.Drawing.Point(488, 169);
            button3.Margin = new System.Windows.Forms.Padding(2);
            button3.Name = "button3";
            button3.Size = new System.Drawing.Size(117, 34);
            button3.TabIndex = 9;
            button3.Text = "OK";
            button3.UseVisualStyleBackColor = true;
            button3.Click += LoadPartitionArray;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(524, 9);
            label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(39, 15);
            label4.TabIndex = 10;
            label4.Text = "Name";
            // 
            // NewPartitionDialog
            // 
            AcceptButton = button1;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            AutoSize = true;
            CancelButton = button2;
            ClientSize = new System.Drawing.Size(872, 220);
            Controls.Add(label4);
            Controls.Add(button3);
            Controls.Add(textBox4);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox3);
            Controls.Add(label3);
            Controls.Add(textBox2);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "NewPartitionDialog";
            ShowIcon = false;
            Text = "NewPartitionDialog";
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        public System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label4;
    }
}