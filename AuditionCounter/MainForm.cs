using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace AuditionCounter
{
    public partial class MainForm : Form
    {
        // Các API Windows cần thiết
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        #region Field Declarations
        // Biến thành viên
        private string aubizPath; // Đường dẫn tới file game
        private string loginCode = "Aubiz.exe /T3ENTER 14007E346F540B57760D5668125966034566045B F"; // Mã đăng nhập
        private IntPtr auditionWindow; // Handle của cửa sổ game
        private bool isMonitoring; // Trạng thái giám sát
        private List<GameRound> gameHistory; // Lịch sử các ván đấu
        private int currentRound; // Số ván hiện tại
        private int initialVcoin; // Số Vcoin ban đầu
        private DateTime startTime; // Thời gian bắt đầu
        private DateTime lastGameRoundTime = DateTime.MinValue; // Thời gian ván gần nhất

        // Các biến mới thêm vào
        private MemoryReader memoryReader;
        private int lastVcoin = 0;
        private System.Threading.Timer memoryMonitorTimer;
        private List<IntPtr> potentialVcoinAddresses = new List<IntPtr>();
        private IntPtr currentVcoinAddress = IntPtr.Zero;

        // Tùy chọn
        private bool autoSaveHistory = true;
        private bool showNotifications = true;
        private int maxRounds = 36;

        // Theo dõi tất cả các địa chỉ tiềm năng
        private Dictionary<IntPtr, int> monitoredAddresses = new Dictionary<IntPtr, int>();
        private bool isFirstScan = true;
        private HashSet<int> recordedVcoinValues = new HashSet<int>();

        // Label thêm vào UI để hiển thị tổng Vcoin kiếm được
        private Label lblTotalEarnedVcoin;

        // Thời gian tối thiểu giữa các ván (giây)
        private const int MIN_ROUND_INTERVAL_SECONDS = 5;
        #endregion

        #region Initialization
        public MainForm()
        {
            InitializeComponent();

            // Khởi tạo menu
            InitializeMenu();

            LoadSettings();
            InitializeMemoryReader();

            // Khởi tạo danh sách lịch sử
            gameHistory = new List<GameRound>();

            // Thêm nút nhập vcoin thủ công
            AddManualInputButton();

            // Thêm label hiển thị tổng Vcoin kiếm được
            AddStatLabels();
        }
        #endregion

        #region UI Methods
        /// <summary>
        /// Thêm labels hiển thị thống kê
        /// </summary>
        private void AddStatLabels()
        {
            // Label hiển thị tổng Vcoin kiếm được
            lblTotalEarnedVcoin = new Label();
            lblTotalEarnedVcoin.Text = "Tổng Vcoin kiếm được: 0";
            lblTotalEarnedVcoin.AutoSize = true;
            lblTotalEarnedVcoin.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            lblTotalEarnedVcoin.ForeColor = Color.Green;
            lblTotalEarnedVcoin.Location = new Point(15, 165);
            this.Controls.Add(lblTotalEarnedVcoin);
        }

        /// <summary>
        /// Thêm nút nhập Vcoin thủ công
        /// </summary>
        private void AddManualInputButton()
        {
            Button btnManualInput = new Button();
            btnManualInput.Text = "Nhập Vcoin thủ công";
            btnManualInput.Size = new Size(245, 30);
            btnManualInput.Location = new Point(12, 220);
            btnManualInput.Click += btnManualInput_Click;
            this.Controls.Add(btnManualInput);
        }

        /// <summary>
        /// Khởi tạo menu chính
        /// </summary>
        private void InitializeMenu()
        {
            // Menu chính
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Dock = DockStyle.Top;

            // Menu File
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Lưu lịch sử", null, (s, e) => SaveGameHistory());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Thoát", null, (s, e) => this.Close());

            // Menu Công cụ
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("Công cụ");
            toolsMenu.DropDownItems.Add("Quét lại địa chỉ Vcoin", null, (s, e) => FindVcoinAddresses(lastVcoin));
            toolsMenu.DropDownItems.Add("Cập nhật Vcoin hiện tại", null, (s, e) => UpdateCurrentVcoin());
            toolsMenu.DropDownItems.Add(new ToolStripSeparator());

            // Submenu phiên bản game
            ToolStripMenuItem versionMenu = new ToolStripMenuItem("Chọn phiên bản game");
            if (memoryReader != null)
            {
                string[] patternNames = memoryReader.GetKnownPatternNames();
                foreach (string name in patternNames)
                {
                    ToolStripMenuItem versionItem = new ToolStripMenuItem(name);
                    versionItem.Click += (s, e) =>
                    {
                        memoryReader.UseKnownPattern(name);
                        UpdateStatus($"Đã chọn pattern cho phiên bản: {name}");

                        // Quét lại địa chỉ Vcoin sau khi đổi pattern
                        if (lastVcoin > 0)
                        {
                            FindVcoinAddresses(lastVcoin);
                        }
                    };
                    versionMenu.DropDownItems.Add(versionItem);
                }
            }
            toolsMenu.DropDownItems.Add(versionMenu);

            // Menu Cài đặt
            ToolStripMenuItem settingsMenu = new ToolStripMenuItem("Cài đặt");

            // Tự động lưu lịch sử
            ToolStripMenuItem autoSaveItem = new ToolStripMenuItem("Tự động lưu lịch sử");
            autoSaveItem.Checked = autoSaveHistory;
            autoSaveItem.CheckOnClick = true;
            autoSaveItem.Click += (s, e) => { autoSaveHistory = autoSaveItem.Checked; };
            settingsMenu.DropDownItems.Add(autoSaveItem);

            // Hiển thị thông báo
            ToolStripMenuItem notifyItem = new ToolStripMenuItem("Hiện thông báo kết quả");
            notifyItem.Checked = showNotifications;
            notifyItem.CheckOnClick = true;
            notifyItem.Click += (s, e) => { showNotifications = notifyItem.Checked; };
            settingsMenu.DropDownItems.Add(notifyItem);

            // Menu Trợ giúp
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Trợ giúp");
            helpMenu.DropDownItems.Add("Hướng dẫn sử dụng", null, (s, e) => ShowHelp());
            helpMenu.DropDownItems.Add("Thông tin", null, (s, e) => ShowAbout());

            // Thêm các menu vào menu chính
            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(toolsMenu);
            mainMenu.Items.Add(settingsMenu);
            mainMenu.Items.Add(helpMenu);

            // Thêm menu vào form
            this.Controls.Add(mainMenu);
            this.MainMenuStrip = mainMenu;
        }

        /// <summary>
        /// Hiển thị hướng dẫn sử dụng (sử dụng form riêng)
        /// </summary>
        private void ShowHelp()
        {
            using (HelpDialog helpDialog = new HelpDialog())
            {
                helpDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Hiển thị thông tin về ứng dụng (sử dụng form riêng)
        /// </summary>
        private void ShowAbout()
        {
            using (AboutDialog aboutDialog = new AboutDialog())
            {
                aboutDialog.ShowDialog(this);
            }
        }

        /// <summary>
        /// Cập nhật giao diện người dùng
        /// </summary>
        private void UpdateUI()
        {
            try
            {
                // Cập nhật số ván hiện tại
                lblCurrentRound.Text = currentRound.ToString();

                // Cập nhật số ván còn lại
                lblRemainingRounds.Text = (maxRounds - currentRound).ToString();

                // Tính tổng Vcoin
                int totalVcoin = initialVcoin;
                int earnedVcoin = 0;
                foreach (GameRound round in gameHistory)
                {
                    earnedVcoin += round.VcoinEarned;
                    totalVcoin += round.VcoinEarned;
                }
                lblTotalVcoin.Text = totalVcoin.ToString();

                // Cập nhật tổng Vcoin kiếm được với font lớn hơn và màu nổi bật hơn
                lblTotalEarnedVcoin.Text = $"Tổng Vcoin kiếm được: +{earnedVcoin}";
                lblTotalEarnedVcoin.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
                lblTotalEarnedVcoin.ForeColor = Color.Green;

                // Cập nhật ListView
                lstGameHistory.Items.Clear();
                foreach (GameRound round in gameHistory)
                {
                    ListViewItem item = new ListViewItem(round.RoundNumber.ToString());
                    item.SubItems.Add("+" + round.VcoinEarned.ToString()); // Thêm dấu + trước giá trị
                    item.SubItems.Add(round.Timestamp.ToString("HH:mm:ss"));
                    if (!string.IsNullOrEmpty(round.Notes))
                    {
                        item.SubItems.Add(round.Notes);
                    }
                    lstGameHistory.Items.Add(item);
                }

                // Tự động cuộn xuống mục mới nhất
                if (lstGameHistory.Items.Count > 0)
                {
                    lstGameHistory.Items[lstGameHistory.Items.Count - 1].EnsureVisible();
                }

                // Tự động lưu lịch sử nếu được bật
                if (autoSaveHistory && gameHistory.Count > 0)
                {
                    SaveGameHistory();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thanh trạng thái
        /// </summary>
        private void UpdateStatus(string message)
        {
            try
            {
                // Đảm bảo cập nhật từ thread UI
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action<string>(UpdateStatus), message);
                    return;
                }

                lblStatus.Text = message;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Nút bắt đầu/dừng theo dõi game
        /// </summary>
        private void btnStartGame_Click(object sender, EventArgs e)
        {
            try
            {
                if (isMonitoring)
                {
                    // Nếu đang theo dõi, dừng lại
                    StopMemoryMonitoring();
                    return;
                }

                // Lưu cài đặt
                aubizPath = txtAubizPath.Text.Trim();
                SaveSettings();

                // Kiểm tra xem file tồn tại không
                if (!File.Exists(aubizPath))
                {
                    MessageBox.Show($"Không tìm thấy file: {aubizPath}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Kiểm tra xem game đã chạy chưa
                Process[] processes = Process.GetProcessesByName("Aubiz");
                if (processes.Length == 0)
                {
                    processes = Process.GetProcessesByName("Audition");
                }

                if (processes.Length == 0)
                {
                    // Game chưa chạy, khởi động game với CMD giống hệt như file .bat
                    string gameDir = Path.GetDirectoryName(aubizPath);

                    // Tạo một process CMD mới
                    Process cmdProcess = new Process();
                    cmdProcess.StartInfo.FileName = "cmd.exe";
                    cmdProcess.StartInfo.Arguments = $"/c cd /d \"{gameDir}\" && {loginCode}";
                    cmdProcess.StartInfo.UseShellExecute = false;
                    cmdProcess.StartInfo.CreateNoWindow = true;
                    cmdProcess.Start();

                    UpdateStatus("Đã khởi động game, đang chờ game khởi động...");

                    // Sử dụng timer để đợi game khởi động
                    System.Windows.Forms.Timer startupTimer = new System.Windows.Forms.Timer();
                    startupTimer.Interval = 8000; // 8 giây để đảm bảo game khởi động đầy đủ
                    startupTimer.Tick += (s, ev) => {
                        startupTimer.Stop();
                        StartMemoryMonitoring();
                    };
                    startupTimer.Start();
                }
                else
                {
                    // Game đã chạy, bắt đầu theo dõi ngay
                    StartMemoryMonitoring();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Sự kiện nút nhập Vcoin thủ công
        /// </summary>
        private void btnManualInput_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("Nhập số Vcoin nhận được từ ván này:", "Nhập thủ công", "0");

            if (int.TryParse(input, out int vcoinEarned) && vcoinEarned > 0)
            {
                currentRound++;

                GameRound round = new GameRound
                {
                    RoundNumber = currentRound,
                    VcoinEarned = vcoinEarned,
                    Timestamp = DateTime.Now
                };

                gameHistory.Add(round);
                UpdateUI();
                UpdateStatus($"Đã thêm ván {currentRound}: +{vcoinEarned}vc");

                // Thông báo khi có ván mới
                if (showNotifications)
                {
                    ShowVcoinNotification(currentRound, vcoinEarned);
                }
            }
        }

        /// <summary>
        /// Khi form tải
        /// </summary>
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Tải cài đặt
                LoadSettings();

                // Nếu đã có đường dẫn, hiển thị
                if (!string.IsNullOrEmpty(aubizPath))
                {
                    txtAubizPath.Text = aubizPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Khi nút Browse được nhấn
        /// </summary>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            try
            {
                // Hiển thị hộp thoại chọn file
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtAubizPath.Text = openFileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Khi form đang đóng
        /// </summary>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Lưu cài đặt
                SaveSettings();

                // Dừng theo dõi bộ nhớ
                if (isMonitoring)
                {
                    StopMemoryMonitoring();
                }

                // Lưu lại địa chỉ Vcoin đã tìm được
                if (memoryReader != null && currentVcoinAddress != IntPtr.Zero)
                {
                    memoryReader.SaveVcoinAddress(currentVcoinAddress);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đóng ứng dụng: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Cập nhật lại giá trị Vcoin hiện tại
        /// </summary>
        private void UpdateCurrentVcoin()
        {
            if (!isMonitoring || memoryReader == null)
            {
                MessageBox.Show("Vui lòng khởi động theo dõi game trước.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (currentVcoinAddress != IntPtr.Zero)
            {
                // Đọc lại giá trị Vcoin từ bộ nhớ
                int currentVcoinValue = memoryReader.ReadInt32FromAddress(currentVcoinAddress);

                if (currentVcoinValue > 0 && currentVcoinValue < 10000000) // Kiểm tra giá trị hợp lý
                {
                    // Cập nhật lại giá trị lastVcoin
                    lastVcoin = currentVcoinValue;
                    UpdateStatus($"Đã cập nhật Vcoin hiện tại: {currentVcoinValue}");
                }
                else
                {
                    MessageBox.Show($"Giá trị Vcoin không hợp lý: {currentVcoinValue}. Hãy thử quét lại địa chỉ Vcoin.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // Hỏi người dùng nhập giá trị Vcoin hiện tại
                string input = Interaction.InputBox("Nhập số Vcoin hiện tại:", "Nhập thủ công", "0");
                if (int.TryParse(input, out int vcoinValue) && vcoinValue > 0)
                {
                    lastVcoin = vcoinValue;
                    FindVcoinAddresses(vcoinValue);
                }
            }
        }

        /// <summary>
        /// Hiển thị thông báo khi nhận được Vcoin
        /// </summary>
        private void ShowVcoinNotification(int roundNumber, int vcoinEarned)
        {
            try
            {
                // Tính tổng Vcoin hiện tại
                int totalVcoin = initialVcoin;
                foreach (GameRound round in gameHistory)
                {
                    totalVcoin += round.VcoinEarned;
                }

                // Hiển thị thông báo bong bóng với nhiều thông tin hơn
                notifyIcon.BalloonTipTitle = $"Ván {roundNumber} hoàn thành";
                notifyIcon.BalloonTipText = $"Nhận được +{vcoinEarned} Vcoin\nTổng Vcoin: {totalVcoin}\nSố ván còn lại: {maxRounds - roundNumber}";
                notifyIcon.ShowBalloonTip(3000);

                // Phát âm thanh thông báo (tùy chọn)
                System.Media.SystemSounds.Asterisk.Play();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi hiển thị thông báo: {ex.Message}");
            }
        }
        #endregion

        #region Memory Methods
        /// <summary>
        /// Khởi tạo đối tượng MemoryReader
        /// </summary>
        private void InitializeMemoryReader()
        {
            memoryReader = new MemoryReader();

            // Đăng ký hàm callback để hiển thị trạng thái
            memoryReader.UpdateStatus = new MemoryReader.UpdateStatusDelegate(this.UpdateStatus);
        }

        /// <summary>
        /// Bắt đầu theo dõi bộ nhớ game
        /// </summary>
        private void StartMemoryMonitoring()
        {
            try
            {
                // Tìm cửa sổ game Audition
                auditionWindow = FindWindow(null, "Audition - AuBiz");
                if (auditionWindow == IntPtr.Zero)
                {
                    auditionWindow = FindWindow(null, "Audition");
                }

                if (auditionWindow == IntPtr.Zero)
                {
                    MessageBox.Show("Không tìm thấy cửa sổ game Audition! Hãy đảm bảo game đang chạy.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (memoryReader == null)
                {
                    InitializeMemoryReader();
                }

                // Kết nối đến tiến trình game
                UpdateStatus("Đang kết nối đến game...");

                if (!memoryReader.Connect())
                {
                    MessageBox.Show("Không thể kết nối đến game Audition. Hãy đảm bảo game đang chạy và ứng dụng có quyền quản trị.",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Ghi nhận thời gian bắt đầu
                startTime = DateTime.Now;

                // Thử tải địa chỉ Vcoin đã lưu trước đó
                IntPtr savedAddress = memoryReader.LoadVcoinAddress();
                if (savedAddress != IntPtr.Zero)
                {
                    // Đã tìm thấy địa chỉ đã lưu, kiểm tra xem địa chỉ còn hợp lệ không
                    currentVcoinAddress = savedAddress;
                    int savedVcoin = memoryReader.ReadInt32FromAddress(currentVcoinAddress);

                    if (savedVcoin > 0 && savedVcoin < 10000000) // Giá trị hợp lý
                    {
                        initialVcoin = savedVcoin;
                        lastVcoin = savedVcoin;
                        UpdateStatus($"Đã tải địa chỉ Vcoin từ lần sử dụng trước. Vcoin hiện tại: {savedVcoin}");
                    }
                    else
                    {
                        // Địa chỉ không còn hợp lệ, cần quét lại
                        currentVcoinAddress = IntPtr.Zero;
                    }
                }

                // Nếu không tải được địa chỉ đã lưu, hỏi người dùng nhập
                if (currentVcoinAddress == IntPtr.Zero)
                {
                    // Đọc Vcoin ban đầu (thử đọc tự động)
                    int initialVcoinValue = memoryReader.ReadVcoin();

                    // Nếu không đọc được, hỏi người dùng
                    if (initialVcoinValue <= 0)
                    {
                        // Hiển thị hộp thoại yêu cầu nhập Vcoin
                        string input = Microsoft.VisualBasic.Interaction.InputBox(
                            "Vui lòng nhập số Vcoin hiện tại trong game:",
                            "Nhập Vcoin ban đầu", "0");

                        if (int.TryParse(input, out initialVcoinValue) && initialVcoinValue > 0)
                        {
                            initialVcoin = initialVcoinValue;
                            lastVcoin = initialVcoinValue;
                        }
                        else
                        {
                            MessageBox.Show("Vcoin không hợp lệ. Vui lòng khởi động lại ứng dụng.",
                                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Tìm kiếm địa chỉ Vcoin
                        if (initialVcoin > 0)
                        {
                            FindVcoinAddresses(initialVcoin);
                        }
                    }
                    else
                    {
                        initialVcoin = initialVcoinValue;
                        lastVcoin = initialVcoinValue;
                    }
                }

                // Khởi tạo dữ liệu
                currentRound = 0;
                gameHistory.Clear();
                lstGameHistory.Items.Clear();
                isFirstScan = true;
                monitoredAddresses.Clear();
                recordedVcoinValues.Clear();

                // Cập nhật giao diện
                lblInitialVcoin.Text = initialVcoin.ToString();
                lblCurrentRound.Text = "0";
                lblRemainingRounds.Text = maxRounds.ToString();
                lblTotalVcoin.Text = initialVcoin.ToString();
                lblTotalEarnedVcoin.Text = "Tổng Vcoin kiếm được: 0";

                // Đánh dấu đang chạy
                isMonitoring = true;
                btnStartGame.Text = "Dừng theo dõi";

                // Khởi tạo timer để theo dõi bộ nhớ
                memoryMonitorTimer = new System.Threading.Timer(MonitorMemory, null, 0, 500); // kiểm tra mỗi 0.5 giây

                UpdateStatus("Đã bắt đầu theo dõi bộ nhớ game.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi bắt đầu theo dõi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Dừng theo dõi bộ nhớ game
        /// </summary>
        private void StopMemoryMonitoring()
        {
            try
            {
                // Hủy timer nếu đang chạy
                if (memoryMonitorTimer != null)
                {
                    memoryMonitorTimer.Dispose();
                    memoryMonitorTimer = null;
                }

                // Ngắt kết nối đến game
                if (memoryReader != null)
                {
                    memoryReader.Disconnect();
                }

                // Cập nhật trạng thái
                isMonitoring = false;
                btnStartGame.Text = "Khởi động và theo dõi";
                UpdateStatus("Đã dừng theo dõi bộ nhớ game.");

                // Lưu lịch sử
                if (gameHistory.Count > 0 && autoSaveHistory)
                {
                    SaveGameHistory();
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Lỗi khi dừng theo dõi: " + ex.Message);
            }
        }

        /// <summary>
        /// Hàm callback của timer theo dõi bộ nhớ - Đã cải tiến để phát hiện Vcoin nhanh hơn
        /// </summary>
        private void MonitorMemory(object state)
        {
            try
            {
                if (!isMonitoring)
                {
                    return;
                }

                // Kiểm tra xem game còn chạy không
                if (!memoryReader.IsProcessRunning())
                {
                    this.Invoke(new Action(() =>
                    {
                        StopMemoryMonitoring();
                        UpdateStatus("Game đã đóng.");
                    }));
                    return;
                }

                // Cách 1: Sử dụng địa chỉ đã biết
                if (currentVcoinAddress != IntPtr.Zero)
                {
                    int currentVcoinValue = memoryReader.ReadInt32FromAddress(currentVcoinAddress);

                    // Chỉ tiếp tục nếu giá trị đọc được hợp lý
                    if (currentVcoinValue <= 0 || currentVcoinValue > 100000000)
                    {
                        this.Invoke(new Action(() =>
                        {
                            UpdateStatus($"Giá trị Vcoin không hợp lý: {currentVcoinValue}. Đang thử quét lại...");
                        }));

                        // Đặt lại địa chỉ và chuyển sang quét lại
                        currentVcoinAddress = IntPtr.Zero;
                        isFirstScan = true;
                        return;
                    }

                    // Hiển thị giá trị Vcoin hiện tại
                    this.Invoke(new Action(() =>
                    {
                        lblStatus.Text = $"Vcoin hiện tại: {currentVcoinValue} | Địa chỉ: {currentVcoinAddress.ToString("X")}";
                    }));

                    // Kiểm tra kết quả ván đấu (dựa vào sự thay đổi Vcoin)
                    if (lastVcoin > 0 && currentVcoinValue > lastVcoin) // Chỉ xét khi VCoin tăng
                    {
                        int vcoinDifference = currentVcoinValue - lastVcoin;

                        // Nếu Vcoin tăng lên (nhận thưởng sau ván đấu)
                        if (vcoinDifference > 0 && vcoinDifference < 10000) // Giới hạn hợp lý cho tăng
                        {
                            // Kiểm tra xem giá trị này đã được ghi nhận chưa
                            if (!recordedVcoinValues.Contains(currentVcoinValue))
                            {
                                // Kiểm tra thời gian giữa các ván
                                TimeSpan timeSinceLastRound = DateTime.Now - lastGameRoundTime;

                                // Nếu đã đủ thời gian hoặc là ván đầu tiên
                                if (timeSinceLastRound.TotalSeconds >= MIN_ROUND_INTERVAL_SECONDS || lastGameRoundTime == DateTime.MinValue)
                                {
                                    // Cập nhật UI từ thread chính
                                    this.Invoke(new Action(() =>
                                    {
                                        // Tăng số ván
                                        currentRound++;

                                        // Thêm vào lịch sử
                                        GameRound round = new GameRound
                                        {
                                            RoundNumber = currentRound,
                                            VcoinEarned = vcoinDifference,
                                            Timestamp = DateTime.Now
                                        };

                                        gameHistory.Add(round);

                                        // Cập nhật giao diện
                                        UpdateUI();
                                        UpdateStatus($"Phát hiện trong phòng chơi: Ván {currentRound} + {vcoinDifference}vc");

                                        // Hiển thị thông báo nếu tùy chọn được bật
                                        if (showNotifications)
                                        {
                                            ShowVcoinNotification(currentRound, vcoinDifference);
                                        }

                                        // Cập nhật thời gian và giá trị đã ghi nhận
                                        lastGameRoundTime = DateTime.Now;
                                        recordedVcoinValues.Add(currentVcoinValue);
                                    }));
                                }
                                else
                                {
                                    // Thông báo thời gian giữa các ván chưa đủ
                                    this.Invoke(new Action(() =>
                                    {
                                        UpdateStatus($"Phát hiện thay đổi Vcoin +{vcoinDifference}, chờ {MIN_ROUND_INTERVAL_SECONDS - (int)timeSinceLastRound.TotalSeconds}s nữa...");
                                    }));
                                }
                            }
                        }
                    }
                    else if (lastVcoin > 0 && currentVcoinValue < lastVcoin)
                    {
                        // Nếu Vcoin giảm đi (có thể do chi tiêu trong game)
                        int vcoinDifference = currentVcoinValue - lastVcoin;
                        this.Invoke(new Action(() =>
                        {
                            UpdateStatus($"Phát hiện Vcoin giảm: {vcoinDifference}vc (Có thể do chi tiêu trong game)");
                        }));
                    }

                    // Lưu giá trị Vcoin hiện tại để so sánh sau
                    lastVcoin = currentVcoinValue;
                    return;
                }

                // Cách 2: Quét toàn bộ bộ nhớ và theo dõi giá trị thay đổi
                if (isFirstScan)
                {
                    // Lần quét đầu tiên, lưu tất cả các giá trị int trong bộ nhớ
                    if (lastVcoin <= 0)
                    {
                        // Nếu chưa có giá trị Vcoin ban đầu, thử đọc từ các địa chỉ tiềm năng
                        lastVcoin = initialVcoin > 0 ? initialVcoin : 0;
                    }

                    if (lastVcoin > 0)
                    {
                        potentialVcoinAddresses = memoryReader.ScanForVcoinAddresses(lastVcoin);

                        foreach (IntPtr addr in potentialVcoinAddresses)
                        {
                            int value = memoryReader.ReadInt32FromAddress(addr);
                            if (value > 0 && value < 10000000) // Giá trị Vcoin hợp lý
                            {
                                monitoredAddresses[addr] = value;
                            }
                        }

                        isFirstScan = false;
                        this.Invoke(new Action(() =>
                        {
                            UpdateStatus($"Đang theo dõi {monitoredAddresses.Count} địa chỉ tiềm năng.");
                        }));
                    }
                }
                else
                {
                    // Lần quét tiếp theo, kiểm tra các địa chỉ đã lưu
                    List<IntPtr> changedAddresses = new List<IntPtr>();

                    foreach (var pair in monitoredAddresses.ToList())
                    {
                        IntPtr addr = pair.Key;
                        int oldValue = pair.Value;

                        int newValue = memoryReader.ReadInt32FromAddress(addr);

                        // Nếu giá trị thay đổi và tăng lên
                        if (newValue > oldValue && newValue < oldValue + 10000)
                        {
                            changedAddresses.Add(addr);
                            monitoredAddresses[addr] = newValue;

                            // Hiển thị các địa chỉ đã thay đổi
                            this.Invoke(new Action(() =>
                            {
                                UpdateStatus($"Phát hiện thay đổi: {addr.ToString("X")} từ {oldValue} thành {newValue}");

                                // Nếu chưa lưu địa chỉ Vcoin
                                if (currentVcoinAddress == IntPtr.Zero)
                                {
                                    // Lưu địa chỉ Vcoin
                                    SetVcoinAddress(addr, newValue);

                                    // Cập nhật UI
                                    currentRound++;
                                    GameRound round = new GameRound
                                    {
                                        RoundNumber = currentRound,
                                        VcoinEarned = newValue - oldValue,
                                        Timestamp = DateTime.Now
                                    };
                                    gameHistory.Add(round);
                                    UpdateUI();

                                    // Hiển thị thông báo
                                    if (showNotifications)
                                    {
                                        ShowVcoinNotification(currentRound, newValue - oldValue);
                                    }
                                }
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi trong MonitorMemory: " + ex.Message);
            }
        }

        /// <summary>
        /// Tìm kiếm địa chỉ Vcoin trong bộ nhớ
        /// </summary>
        private void FindVcoinAddresses(int vcoinValue)
        {
            try
            {
                UpdateStatus("Đang tìm kiếm địa chỉ Vcoin...");

                if (memoryReader == null || !memoryReader.IsProcessRunning())
                {
                    UpdateStatus("Không thể tìm kiếm: Game chưa được khởi động.");
                    return;
                }

                // Tìm kiếm địa chỉ chứa giá trị Vcoin
                potentialVcoinAddresses = memoryReader.ScanForVcoinAddresses(vcoinValue);

                // Thêm vào danh sách giám sát
                monitoredAddresses.Clear();
                foreach (IntPtr addr in potentialVcoinAddresses)
                {
                    int value = memoryReader.ReadInt32FromAddress(addr);
                    if (value > 0 && value < 10000000) // Giá trị Vcoin hợp lý
                    {
                        monitoredAddresses[addr] = value;
                    }
                }

                UpdateStatus($"Đã tìm thấy {monitoredAddresses.Count} địa chỉ tiềm năng.");
                isFirstScan = false;
            }
            catch (Exception ex)
            {
                UpdateStatus($"Lỗi khi tìm kiếm: {ex.Message}");
            }
        }

        /// <summary>
        /// Thiết lập địa chỉ Vcoin và cập nhật trạng thái
        /// </summary>
        public void SetVcoinAddress(IntPtr address, int currentValue)
        {
            currentVcoinAddress = address;
            initialVcoin = currentValue;
            lastVcoin = currentValue;
            lastGameRoundTime = DateTime.MinValue; // Đặt lại thời gian ván gần nhất
            recordedVcoinValues.Clear(); // Đặt lại danh sách giá trị đã ghi nhận

            // Cập nhật UI
            lblInitialVcoin.Text = initialVcoin.ToString();
            lblTotalVcoin.Text = initialVcoin.ToString();
            lblTotalEarnedVcoin.Text = "Tổng Vcoin kiếm được: 0";

            // Lưu địa chỉ để dùng sau
            if (memoryReader != null)
            {
                memoryReader.SaveVcoinAddress(address);
            }

            UpdateStatus($"Đã cập nhật địa chỉ Vcoin: {address.ToString("X")} với giá trị: {currentValue}");
        }
        #endregion

        #region Data Methods
        /// <summary>
        /// Lưu lịch sử ván đấu
        /// </summary>
        private void SaveGameHistory()
        {
            try
            {
                if (gameHistory.Count == 0)
                {
                    return;
                }

                // Tạo thư mục lưu trữ nếu chưa tồn tại
                string folderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "AuditionCounter");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Tạo tên file dựa trên ngày tháng
                string fileName = $"GameHistory_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                string filePath = Path.Combine(folderPath, fileName);

                // Ghi dữ liệu
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    writer.WriteLine($"AuditionCounter - Lịch sử ván đấu - {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}");
                    writer.WriteLine($"Vcoin Ban đầu: {initialVcoin}");
                    writer.WriteLine("-----------------------------------------");

                    int totalVcoin = initialVcoin;
                    foreach (GameRound round in gameHistory)
                    {
                        totalVcoin += round.VcoinEarned;
                        writer.WriteLine($"Ván {round.RoundNumber}: +{round.VcoinEarned}vc ({round.Timestamp.ToString("HH:mm:ss")})");
                    }

                    writer.WriteLine("-----------------------------------------");
                    writer.WriteLine($"Tổng số ván: {gameHistory.Count}");
                    writer.WriteLine($"Tổng Vcoin: {totalVcoin}");
                    writer.WriteLine($"Vcoin trung bình mỗi ván: {(gameHistory.Count > 0 ? (double)gameHistory.Sum(r => r.VcoinEarned) / gameHistory.Count : 0):F2}");
                }

                UpdateStatus($"Đã lưu lịch sử vào {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lưu lịch sử: {ex.Message}");
            }
        }

        /// <summary>
        /// Phương thức lấy đường dẫn đã lưu
        /// </summary>
        private string GetSavedPath()
        {
            try
            {
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AuditionCounter",
                    "path.txt");

                if (File.Exists(filePath))
                    return File.ReadAllText(filePath);
                return "";
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Phương thức lưu đường dẫn
        /// </summary>
        private void SavePath(string path)
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AuditionCounter");

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                string filePath = Path.Combine(dir, "path.txt");
                File.WriteAllText(filePath, path);
            }
            catch { }
        }

        /// <summary>
        /// Tải cài đặt từ ứng dụng
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // Tải đường dẫn Aubiz đã lưu
                aubizPath = GetSavedPath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải cài đặt: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu cài đặt ứng dụng
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // Lưu đường dẫn Aubiz
                SavePath(txtAubizPath.Text.Trim());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lưu cài đặt: {ex.Message}");
            }
        }
        #endregion
    }
}