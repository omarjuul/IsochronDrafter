namespace IsochronDrafter
{
    partial class ServerWindow
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
            this.button1 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lbFile = new System.Windows.Forms.Label();
            this.tbFile = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.lbLands = new System.Windows.Forms.Label();
            this.tbLands = new System.Windows.Forms.TextBox();
            this.lbNonLands = new System.Windows.Forms.Label();
            this.tbNonLands = new System.Windows.Forms.TextBox();
            this.lbPacks = new System.Windows.Forms.Label();
            this.tbPacks = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.button1.Location = new System.Drawing.Point(268, 450);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Launch Server";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Isochron Drafter csv files|*.csv";
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(375, 450);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Start Draft";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Window;
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(725, 385);
            this.textBox1.TabIndex = 0;
            // 
            // lbFile
            // 
            this.lbFile.AutoSize = true;
            this.lbFile.Location = new System.Drawing.Point(51, 404);
            this.lbFile.Name = "lbFile";
            this.lbFile.Size = new System.Drawing.Size(45, 13);
            this.lbFile.TabIndex = 3;
            this.lbFile.Text = "Set File:";
            // 
            // tbFile
            // 
            this.tbFile.Location = new System.Drawing.Point(102, 401);
            this.tbFile.Name = "tbFile";
            this.tbFile.Size = new System.Drawing.Size(554, 20);
            this.tbFile.TabIndex = 0;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(662, 399);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Browse...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // lbLands
            // 
            this.lbLands.AutoSize = true;
            this.lbLands.Location = new System.Drawing.Point(176, 427);
            this.lbLands.Name = "lbLands";
            this.lbLands.Size = new System.Drawing.Size(86, 13);
            this.lbLands.TabIndex = 5;
            this.lbLands.Text = "Lands Per Pack:";
            // 
            // tbLands
            // 
            this.tbLands.Location = new System.Drawing.Point(268, 424);
            this.tbLands.Name = "tbLands";
            this.tbLands.Size = new System.Drawing.Size(28, 20);
            this.tbLands.TabIndex = 6;
            // 
            // lbNonLands
            // 
            this.lbNonLands.AutoSize = true;
            this.lbNonLands.Location = new System.Drawing.Point(302, 427);
            this.lbNonLands.Name = "lbNonLands";
            this.lbNonLands.Size = new System.Drawing.Size(55, 13);
            this.lbNonLands.TabIndex = 5;
            this.lbNonLands.Text = "Nonlands:";
            // 
            // tbNonLands
            // 
            this.tbNonLands.Location = new System.Drawing.Point(363, 424);
            this.tbNonLands.Name = "tbNonLands";
            this.tbNonLands.Size = new System.Drawing.Size(31, 20);
            this.tbNonLands.TabIndex = 6;
            // 
            // lbPacks
            // 
            this.lbPacks.AutoSize = true;
            this.lbPacks.Location = new System.Drawing.Point(99, 427);
            this.lbPacks.Name = "lbPacks";
            this.lbPacks.Size = new System.Drawing.Size(40, 13);
            this.lbPacks.TabIndex = 5;
            this.lbPacks.Text = "Packs:";
            // 
            // tbPacks
            // 
            this.tbPacks.Location = new System.Drawing.Point(145, 424);
            this.tbPacks.Name = "tbPacks";
            this.tbPacks.Size = new System.Drawing.Size(25, 20);
            this.tbPacks.TabIndex = 6;
            // 
            // ServerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(749, 506);
            this.Controls.Add(this.tbNonLands);
            this.Controls.Add(this.tbPacks);
            this.Controls.Add(this.tbLands);
            this.Controls.Add(this.lbPacks);
            this.Controls.Add(this.lbNonLands);
            this.Controls.Add(this.lbLands);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.tbFile);
            this.Controls.Add(this.lbFile);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ServerWindow";
            this.Text = "Isochron Drafter Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lbFile;
        private System.Windows.Forms.TextBox tbFile;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label lbLands;
        private System.Windows.Forms.TextBox tbLands;
        private System.Windows.Forms.Label lbNonLands;
        private System.Windows.Forms.TextBox tbNonLands;
        private System.Windows.Forms.Label lbPacks;
        private System.Windows.Forms.TextBox tbPacks;
    }
}