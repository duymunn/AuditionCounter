namespace AuditionCounter
{
    partial class MainForm
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                // Dọn dẹp tài nguyên MemoryReader
                if (memoryMonitorTimer != null)
                {
                    memoryMonitorTimer.Dispose();
                    memoryMonitorTimer = null;
                }

                if (memoryReader != null)
                {
                    memoryReader.Disconnect();
                    memoryReader = null;
                }
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

            // Các thành phần UI - Thay đổi khai báo từ private thành public
            this.lblPathLabel = new System.Windows.Forms.Label();
            this.txtAubizPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnStartGame = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblTotalVcoin = new System.Windows.Forms.Label();
            this.lblRemainingRounds = new System.Windows.Forms.Label();
            this.lblCurrentRound = new System.Windows.Forms.Label();
            this.lblInitialVcoin = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lstGameHistory = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();

            // lblPathLabel
            this.lblPathLabel.AutoSize = true;
            this.lblPathLabel.Location = new System.Drawing.Point(12, 42);
            this.lblPathLabel.Name = "lblPathLabel";
            this.lblPathLabel.Size = new System.Drawing.Size(80, 13);
            this.lblPathLabel.TabIndex = 0;
            this.lblPathLabel.Text = "Đường dẫn game:";

            // txtAubizPath
            this.txtAubizPath.Location = new System.Drawing.Point(98, 39);
            this.txtAubizPath.Name = "txtAubizPath";
            this.txtAubizPath.Size = new System.Drawing.Size(367, 20);
            this.txtAubizPath.TabIndex = 1;

            // btnBrowse
            this.btnBrowse.Location = new System.Drawing.Point(471, 38);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(32, 22);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            // btnStartGame
            this.btnStartGame.Location = new System.Drawing.Point(509, 38);
            this.btnStartGame.Name = "btnStartGame";
            this.btnStartGame.Size = new System.Drawing.Size(147, 23);
            this.btnStartGame.TabIndex = 3;
            this.btnStartGame.Text = "Khởi động và theo dõi";
            this.btnStartGame.UseVisualStyleBackColor = true;
            this.btnStartGame.Click += new System.EventHandler(this.btnStartGame_Click);

            // groupBox1
            this.groupBox1.Controls.Add(this.lblTotalVcoin);
            this.groupBox1.Controls.Add(this.lblRemainingRounds);
            this.groupBox1.Controls.Add(this.lblCurrentRound);
            this.groupBox1.Controls.Add(this.lblInitialVcoin);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 67);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(245, 132);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Thông tin";

            // lblTotalVcoin
            this.lblTotalVcoin.AutoSize = true;
            this.lblTotalVcoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalVcoin.ForeColor = System.Drawing.Color.Blue;
            this.lblTotalVcoin.Location = new System.Drawing.Point(109, 100);
            this.lblTotalVcoin.Name = "lblTotalVcoin";
            this.lblTotalVcoin.Size = new System.Drawing.Size(14, 13);
            this.lblTotalVcoin.TabIndex = 7;
            this.lblTotalVcoin.Text = "0";

            // lblRemainingRounds
            this.lblRemainingRounds.AutoSize = true;
            this.lblRemainingRounds.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemainingRounds.Location = new System.Drawing.Point(109, 75);
            this.lblRemainingRounds.Name = "lblRemainingRounds";
            this.lblRemainingRounds.Size = new System.Drawing.Size(21, 13);
            this.lblRemainingRounds.TabIndex = 6;
            this.lblRemainingRounds.Text = "36";

            // lblCurrentRound
            this.lblCurrentRound.AutoSize = true;
            this.lblCurrentRound.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentRound.Location = new System.Drawing.Point(109, 50);
            this.lblCurrentRound.Name = "lblCurrentRound";
            this.lblCurrentRound.Size = new System.Drawing.Size(14, 13);
            this.lblCurrentRound.TabIndex = 5;
            this.lblCurrentRound.Text = "0";

            // lblInitialVcoin
            this.lblInitialVcoin.AutoSize = true;
            this.lblInitialVcoin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInitialVcoin.Location = new System.Drawing.Point(109, 25);
            this.lblInitialVcoin.Name = "lblInitialVcoin";
            this.lblInitialVcoin.Size = new System.Drawing.Size(14, 13);
            this.lblInitialVcoin.TabIndex = 4;
            this.lblInitialVcoin.Text = "0";

            // label4
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 100);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Tổng Vcoin:";

            // label3
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Số ván còn lại:";

            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Số ván đã chơi:";

            // label1
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Vcoin ban đầu:";

            // groupBox2
            this.groupBox2.Controls.Add(this.lstGameHistory);
            this.groupBox2.Location = new System.Drawing.Point(263, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(393, 200);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Lịch sử ván đấu";

            // lstGameHistory
            this.lstGameHistory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnHeader1,
                this.columnHeader2,
                this.columnHeader3});
            this.lstGameHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstGameHistory.FullRowSelect = true;
            this.lstGameHistory.GridLines = true;
            this.lstGameHistory.HideSelection = false;
            this.lstGameHistory.Location = new System.Drawing.Point(3, 16);
            this.lstGameHistory.Name = "lstGameHistory";
            this.lstGameHistory.Size = new System.Drawing.Size(387, 181);
            this.lstGameHistory.TabIndex = 0;
            this.lstGameHistory.UseCompatibleStateImageBehavior = false;
            this.lstGameHistory.View = System.Windows.Forms.View.Details;

            // columnHeader1
            this.columnHeader1.Text = "Ván";
            this.columnHeader1.Width = 40;

            // columnHeader2
            this.columnHeader2.Text = "Vcoin";
            this.columnHeader2.Width = 80;

            // columnHeader3
            this.columnHeader3.Text = "Thời gian";
            this.columnHeader3.Width = 120;

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.lblStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 275);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(668, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";

            // lblStatus
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(67, 17);
            this.lblStatus.Text = "Sẵn sàng...";

            // notifyIcon
            this.notifyIcon.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.notifyIcon.BalloonTipText = "Ứng dụng theo dõi Audition đang chạy";
            this.notifyIcon.BalloonTipTitle = "Audition Counter";
            this.notifyIcon.Icon = System.Drawing.SystemIcons.Application;
            this.notifyIcon.Text = "Audition Counter";
            this.notifyIcon.Visible = true;

            // openFileDialog
            this.openFileDialog.DefaultExt = "exe";
            this.openFileDialog.FileName = "Aubiz.exe";
            this.openFileDialog.Filter = "Game Audition (*.exe)|*.exe|Tất cả file (*.*)|*.*";
            this.openFileDialog.Title = "Chọn file game Audition";

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(668, 297);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnStartGame);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtAubizPath);
            this.Controls.Add(this.lblPathLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audition Counter - Theo dõi Vcoin";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        // Thay đổi từ private sang public cho tất cả điều khiển
        public System.Windows.Forms.Label lblPathLabel;
        public System.Windows.Forms.TextBox txtAubizPath;
        public System.Windows.Forms.Button btnBrowse;
        public System.Windows.Forms.Button btnStartGame;
        public System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.Label label4;
        public System.Windows.Forms.Label label3;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label label1;
        public System.Windows.Forms.Label lblTotalVcoin;
        public System.Windows.Forms.Label lblRemainingRounds;
        public System.Windows.Forms.Label lblCurrentRound;
        public System.Windows.Forms.Label lblInitialVcoin;
        public System.Windows.Forms.GroupBox groupBox2;
        public System.Windows.Forms.ListView lstGameHistory;
        public System.Windows.Forms.ColumnHeader columnHeader1;
        public System.Windows.Forms.ColumnHeader columnHeader2;
        public System.Windows.Forms.ColumnHeader columnHeader3;
        public System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.ToolStripStatusLabel lblStatus;
        public System.Windows.Forms.NotifyIcon notifyIcon;
        public System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        public System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}