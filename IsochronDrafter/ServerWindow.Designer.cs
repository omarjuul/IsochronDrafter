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
            this.btnLaunchServer = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnStartDraft = new System.Windows.Forms.Button();
            this.tbxMessageLog = new System.Windows.Forms.TextBox();
            this.lbFile = new System.Windows.Forms.Label();
            this.tbFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lbLands = new System.Windows.Forms.Label();
            this.tbLands = new System.Windows.Forms.TextBox();
            this.lbNonLands = new System.Windows.Forms.Label();
            this.tbNonLands = new System.Windows.Forms.TextBox();
            this.lbPacks = new System.Windows.Forms.Label();
            this.tbPacks = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnLaunchServer
            // 
            this.btnLaunchServer.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLaunchServer.Location = new System.Drawing.Point(268, 450);
            this.btnLaunchServer.Name = "btnLaunchServer";
            this.btnLaunchServer.Size = new System.Drawing.Size(101, 23);
            this.btnLaunchServer.TabIndex = 1;
            this.btnLaunchServer.Text = "Launch Server";
            this.btnLaunchServer.UseVisualStyleBackColor = true;
            this.btnLaunchServer.Click += new System.EventHandler(this.btnLaunchServer_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Isochron Drafter csv files|*.csv";
            // 
            // btnStartDraft
            // 
            this.btnStartDraft.Enabled = false;
            this.btnStartDraft.Location = new System.Drawing.Point(375, 450);
            this.btnStartDraft.Name = "btnStartDraft";
            this.btnStartDraft.Size = new System.Drawing.Size(85, 23);
            this.btnStartDraft.TabIndex = 2;
            this.btnStartDraft.Text = "Start Draft";
            this.btnStartDraft.UseVisualStyleBackColor = true;
            this.btnStartDraft.Click += new System.EventHandler(this.btnStartDraft_Click);
            // 
            // tbxMessageLog
            // 
            this.tbxMessageLog.BackColor = System.Drawing.SystemColors.Window;
            this.tbxMessageLog.Location = new System.Drawing.Point(12, 12);
            this.tbxMessageLog.Multiline = true;
            this.tbxMessageLog.Name = "tbxMessageLog";
            this.tbxMessageLog.ReadOnly = true;
            this.tbxMessageLog.Size = new System.Drawing.Size(725, 385);
            this.tbxMessageLog.TabIndex = 1;
            this.tbxMessageLog.TabStop = false;
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
            this.tbFile.TabIndex = 3;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(662, 399);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 4;
            this.btnBrowse.Text = "Browse...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lbLands
            // 
            this.lbLands.AutoSize = true;
            this.lbLands.Location = new System.Drawing.Point(176, 427);
            this.lbLands.Name = "lbLands";
            this.lbLands.Size = new System.Drawing.Size(86, 13);
            this.lbLands.TabIndex = 6;
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
            this.lbNonLands.TabIndex = 7;
            this.lbNonLands.Text = "Nonlands:";
            // 
            // tbNonLands
            // 
            this.tbNonLands.Location = new System.Drawing.Point(363, 424);
            this.tbNonLands.Name = "tbNonLands";
            this.tbNonLands.Size = new System.Drawing.Size(31, 20);
            this.tbNonLands.TabIndex = 7;
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
            this.tbPacks.TabIndex = 5;
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
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.tbFile);
            this.Controls.Add(this.lbFile);
            this.Controls.Add(this.btnStartDraft);
            this.Controls.Add(this.btnLaunchServer);
            this.Controls.Add(this.tbxMessageLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ServerWindow";
            this.Text = "Isochron Drafter Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLaunchServer;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button btnStartDraft;
        private System.Windows.Forms.TextBox tbxMessageLog;
        private System.Windows.Forms.Label lbFile;
        private System.Windows.Forms.TextBox tbFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lbLands;
        private System.Windows.Forms.TextBox tbLands;
        private System.Windows.Forms.Label lbNonLands;
        private System.Windows.Forms.TextBox tbNonLands;
        private System.Windows.Forms.Label lbPacks;
        private System.Windows.Forms.TextBox tbPacks;
    }
}