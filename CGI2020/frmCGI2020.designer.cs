namespace CGI2020
{
    partial class frmCGI
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmCGI));
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lstMsg = new System.Windows.Forms.ListBox();
            this.txtInputPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLastInputFile = new System.Windows.Forms.TextBox();
            this.btnTxt2CSV = new System.Windows.Forms.Button();
            this.grpLogs = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.grpLogs.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 300;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // lstMsg
            // 
            this.lstMsg.BackColor = System.Drawing.Color.Black;
            this.lstMsg.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstMsg.ForeColor = System.Drawing.Color.White;
            this.lstMsg.FormattingEnabled = true;
            this.lstMsg.ItemHeight = 16;
            this.lstMsg.Location = new System.Drawing.Point(6, 21);
            this.lstMsg.Name = "lstMsg";
            this.lstMsg.Size = new System.Drawing.Size(974, 532);
            this.lstMsg.TabIndex = 10;
            // 
            // txtInputPath
            // 
            this.txtInputPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtInputPath.Location = new System.Drawing.Point(140, 19);
            this.txtInputPath.Name = "txtInputPath";
            this.txtInputPath.ReadOnly = true;
            this.txtInputPath.Size = new System.Drawing.Size(552, 22);
            this.txtInputPath.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(65, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 16);
            this.label1.TabIndex = 11;
            this.label1.Text = "Input 路徑";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.label2.Location = new System.Drawing.Point(56, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(79, 16);
            this.label2.TabIndex = 12;
            this.label2.Text = "Output 路徑";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtOutputPath.Location = new System.Drawing.Point(140, 49);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.ReadOnly = true;
            this.txtOutputPath.Size = new System.Drawing.Size(552, 22);
            this.txtOutputPath.TabIndex = 13;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.Gainsboro;
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtLastInputFile);
            this.groupBox1.Controls.Add(this.btnTxt2CSV);
            this.groupBox1.Controls.Add(this.txtOutputPath);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtInputPath);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 3F, System.Drawing.FontStyle.Bold);
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.groupBox1.Location = new System.Drawing.Point(12, 33);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(992, 120);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(8, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 16);
            this.label3.TabIndex = 27;
            this.label3.Text = "上回最終處理檔案";
            // 
            // txtLastInputFile
            // 
            this.txtLastInputFile.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.txtLastInputFile.Location = new System.Drawing.Point(141, 81);
            this.txtLastInputFile.Name = "txtLastInputFile";
            this.txtLastInputFile.ReadOnly = true;
            this.txtLastInputFile.Size = new System.Drawing.Size(551, 22);
            this.txtLastInputFile.TabIndex = 26;
            // 
            // btnTxt2CSV
            // 
            this.btnTxt2CSV.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F);
            this.btnTxt2CSV.Location = new System.Drawing.Point(872, 66);
            this.btnTxt2CSV.Name = "btnTxt2CSV";
            this.btnTxt2CSV.Size = new System.Drawing.Size(91, 41);
            this.btnTxt2CSV.TabIndex = 25;
            this.btnTxt2CSV.Text = "手動執行";
            this.btnTxt2CSV.UseVisualStyleBackColor = true;
            this.btnTxt2CSV.Click += new System.EventHandler(this.btnTXT2CSV_Click);
            // 
            // grpLogs
            // 
            this.grpLogs.BackColor = System.Drawing.Color.Gainsboro;
            this.grpLogs.Controls.Add(this.lstMsg);
            this.grpLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.grpLogs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.grpLogs.Location = new System.Drawing.Point(13, 159);
            this.grpLogs.Name = "grpLogs";
            this.grpLogs.Size = new System.Drawing.Size(991, 573);
            this.grpLogs.TabIndex = 26;
            this.grpLogs.TabStop = false;
            this.grpLogs.Text = " >> 檔案處理狀態";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.label5.Location = new System.Drawing.Point(12, 11);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 16);
            this.label5.TabIndex = 28;
            this.label5.Text = "---  CSV  ---";
            // 
            // frmCGI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.grpLogs);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 768);
            this.MinimumSize = new System.Drawing.Size(1024, 768);
            this.Name = "frmCGI";
            this.Text = "CGI資料整理";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmCSV_FormClosing);
            this.Load += new System.EventHandler(this.frmCSV_Load);
            this.Shown += new System.EventHandler(this.frmCSV_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpLogs.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox txtOutputPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtInputPath;
        private System.Windows.Forms.ListBox lstMsg;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox grpLogs;
        private System.Windows.Forms.Button btnTxt2CSV;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLastInputFile;
    }
}

