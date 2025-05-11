using System;
using System.Windows.Forms;
using System.Drawing;

namespace AuditionCounter
{
    public partial class HelpDialog : Form
    {
        public HelpDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Hướng dẫn sử dụng";
            this.Size = new System.Drawing.Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            TextBox txtHelp = new TextBox();
            txtHelp.Multiline = true;
            txtHelp.ReadOnly = true;
            txtHelp.Dock = DockStyle.Fill;
            txtHelp.ScrollBars = ScrollBars.Vertical;
            txtHelp.Text =
                "HƯỚNG DẪN SỬ DỤNG PHẦN MỀM AUDITION COUNTER\r\n\r\n" +
                "1. Cài đặt ban đầu:\r\n" +
                "   - Nhập đường dẫn tới file game Audition/AuBiz\r\n" +
                "   - Nhấn 'Khởi động và theo dõi' để bắt đầu\r\n\r\n" +
                "2. Các chức năng chính:\r\n" +
                "   - Theo dõi số Vcoin nhận được sau mỗi ván đấu\r\n" +
                "   - Hiển thị lịch sử các ván đấu\r\n" +
                "   - Tính toán tổng số Vcoin kiếm được\r\n\r\n" +
                "3. Nhập thủ công:\r\n" +
                "   - Nếu ứng dụng không tự động phát hiện được Vcoin, hãy sử dụng nút 'Nhập Vcoin thủ công'\r\n" +
                "   - Chọn đúng phiên bản game trong menu 'Công cụ' > 'Chọn phiên bản game'\r\n\r\n" +
                "4. Mẹo sử dụng:\r\n" +
                "   - Thử lần lượt các phiên bản game khác nhau nếu không phát hiện được Vcoin\r\n" +
                "   - Sử dụng 'Cập nhật Vcoin hiện tại' từ menu Công cụ nếu giá trị hiển thị không chính xác\r\n" +
                "   - Sử dụng tính năng nhập thủ công nếu các phương pháp tự động không hoạt động\r\n\r\n" +
                "Lưu ý: Ứng dụng cần được chạy với quyền Administrator để đọc bộ nhớ game.";

            Button btnClose = new Button();
            btnClose.Text = "Đóng";
            btnClose.Dock = DockStyle.Bottom;
            btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(txtHelp);
            this.Controls.Add(btnClose);
        }
    }
}