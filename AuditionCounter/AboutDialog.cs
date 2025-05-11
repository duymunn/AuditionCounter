using System;
using System.Windows.Forms;
using System.Drawing;

namespace AuditionCounter
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Thông tin";
            this.Size = new System.Drawing.Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            TextBox txtAbout = new TextBox();
            txtAbout.Multiline = true;
            txtAbout.ReadOnly = true;
            txtAbout.Dock = DockStyle.Fill;
            txtAbout.ScrollBars = ScrollBars.Vertical;
            txtAbout.Text =
                "AUDITION COUNTER\r\n" +
                "Phiên bản: 1.0.0\r\n\r\n" +
                "Phần mềm theo dõi số Vcoin nhận được khi chơi game Audition.\r\n\r\n" +
                "Tính năng chính:\r\n" +
                "- Tự động đọc bộ nhớ game để theo dõi Vcoin\r\n" +
                "- Hiển thị lịch sử các ván đấu\r\n" +
                "- Tính toán tổng số Vcoin kiếm được\r\n\r\n" +
                "© 2025 Duy Munn";

            Button btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(txtAbout);
            this.Controls.Add(btnClose);
        }
    }
}