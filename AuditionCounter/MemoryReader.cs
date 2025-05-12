using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AuditionCounter
{
    public class MemoryReader
    {
        // Các API của Windows để đọc bộ nhớ từ tiến trình khác
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        // Các hằng số
        private const int PROCESS_WM_READ = 0x0010;
        private const int PROCESS_VM_WRITE = 0x0020;
        private const int PROCESS_VM_OPERATION = 0x0008;
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        // Biến thành viên
        private Process gameProcess;
        private IntPtr processHandle = IntPtr.Zero;
        private IntPtr vcoinBaseAddress = IntPtr.Zero;
        private IntPtr[] vcoinOffsets = null;

        // Delegate để báo cáo trạng thái
        public delegate void UpdateStatusDelegate(string message);
        public UpdateStatusDelegate UpdateStatus { get; set; }

        // Các pattern đã biết có thể sử dụng để tìm Vcoin
        private static readonly Dictionary<string, int[]> KnownPatterns = new Dictionary<string, int[]>
        {
            // Tên phiên bản, mảng offset
            { "AuBiz 2023", new int[] { 0x012A36E0, 0x30, 0x120, 0x64, 0x7C } },
            { "Audition VN 2021", new int[] { 0x00F23580, 0x64, 0x18, 0x30, 0x10 } },
            { "AuBiz 2022", new int[] { 0x00DAFC78, 0x3C, 0x28, 0x10, 0x4C } },
            { "AuBiz 2024", new int[] { 0x01357480, 0x18, 0x50, 0x40, 0x24 } },
            // Các pattern mới - hãy thử khi các pattern cũ không hoạt động
            { "New Pattern 1", new int[] { 0x01468C30, 0x20, 0x10, 0x40, 0x24 } },
            { "New Pattern 2", new int[] { 0x00FA6444, 0x28, 0x0C, 0x38, 0x14 } },
            { "New Pattern 3", new int[] { 0x01500000, 0x24, 0x08, 0x44, 0x18 } },
            { "AuBiz 2025", new int[] { 0x01572A8C, 0x14, 0x20, 0x30, 0x28 } },
            { "AuBiz Lite", new int[] { 0x00CEA730, 0x10, 0x40, 0x2C, 0x18 } },
        };

        public MemoryReader()
        {
            // Mặc định sử dụng pattern đầu tiên trong danh sách
            if (KnownPatterns.Count > 0)
            {
                var firstPattern = KnownPatterns.First();
                int[] offsets = firstPattern.Value;

                vcoinOffsets = new IntPtr[offsets.Length];
                for (int i = 0; i < offsets.Length; i++)
                {
                    vcoinOffsets[i] = (IntPtr)offsets[i];
                }
            }
            else
            {
                // Offset mặc định nếu không có pattern nào được cấu hình
                vcoinOffsets = new IntPtr[] {
                    (IntPtr)0x012A36E0, // Địa chỉ cơ sở Vcoin (ví dụ)
                    (IntPtr)0x30, // Offset 1
                    (IntPtr)0x120, // Offset 2
                    (IntPtr)0x64, // Offset 3
                    (IntPtr)0x7C, // Offset 4
                };
            }
        }

        /// <summary>
        /// Cấu hình các offset Vcoin
        /// </summary>
        /// <param name="offsets">Mảng offset để sử dụng</param>
        public void ConfigureOffsets(int[] offsets)
        {
            if (offsets == null || offsets.Length == 0)
            {
                return;
            }

            vcoinOffsets = new IntPtr[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                vcoinOffsets[i] = (IntPtr)offsets[i];
            }
        }

        /// <summary>
        /// Sử dụng pattern đã biết dựa trên tên phiên bản
        /// </summary>
        /// <param name="versionName">Tên phiên bản trong danh sách KnownPatterns</param>
        /// <returns>True nếu tìm thấy và áp dụng pattern</returns>
        public bool UseKnownPattern(string versionName)
        {
            if (KnownPatterns.ContainsKey(versionName))
            {
                ConfigureOffsets(KnownPatterns[versionName]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lấy danh sách tên các pattern đã biết
        /// </summary>
        public string[] GetKnownPatternNames()
        {
            return KnownPatterns.Keys.ToArray();
        }

        /// <summary>
        /// Kết nối đến tiến trình game Audition
        /// </summary>
        /// <returns>True nếu kết nối thành công</returns>
        public bool Connect()
        {
            try
            {
                // Tìm tiến trình game
                Process[] processes = Process.GetProcessesByName("Aubiz");
                if (processes.Length == 0)
                {
                    processes = Process.GetProcessesByName("Audition");
                }

                if (processes.Length == 0)
                {
                    return false;
                }

                gameProcess = processes[0];

                // Mở tiến trình với quyền đọc bộ nhớ
                processHandle = OpenProcess(PROCESS_WM_READ, false, gameProcess.Id);
                if (processHandle == IntPtr.Zero)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi kết nối: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối với tiến trình game
        /// </summary>
        public void Disconnect()
        {
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
            }
            gameProcess = null;
        }

        /// <summary>
        /// Kiểm tra xem tiến trình game còn chạy không
        /// </summary>
        /// <returns>True nếu tiến trình còn chạy</returns>
        public bool IsProcessRunning()
        {
            if (gameProcess == null)
            {
                return false;
            }

            try
            {
                Process.GetProcessById(gameProcess.Id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đọc giá trị Vcoin từ bộ nhớ game
        /// </summary>
        /// <returns>Số Vcoin hiện tại, trả về 0 nếu không đọc được</returns>
        public int ReadVcoin()
        {
            try
            {
                if (processHandle == IntPtr.Zero || gameProcess == null || !IsProcessRunning())
                {
                    return 0;
                }

                // Sử dụng pointer chain để tìm đến địa chỉ Vcoin
                IntPtr baseAddress = gameProcess.MainModule.BaseAddress;
                IntPtr address = IntPtr.Add(baseAddress, (int)vcoinOffsets[0]);

                // Debug
                Debug.WriteLine($"Base Address: {baseAddress.ToString("X")}");
                Debug.WriteLine($"Initial Address: {address.ToString("X")}");

                // Đọc qua chuỗi con trỏ nếu có nhiều offset
                for (int i = 1; i < vcoinOffsets.Length; i++)
                {
                    address = ReadPointerAddress(address);
                    if (address == IntPtr.Zero)
                    {
                        Debug.WriteLine($"Đọc con trỏ thất bại ở offset {i}");
                        return 0;
                    }
                    address = IntPtr.Add(address, (int)vcoinOffsets[i]);
                    Debug.WriteLine($"Địa chỉ sau offset {i}: {address.ToString("X")}");
                }

                // Đọc giá trị int tại địa chỉ cuối cùng
                int value = ReadInt32FromAddress(address);
                Debug.WriteLine($"Giá trị đọc được: {value}");
                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đọc Vcoin: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Đọc địa chỉ từ một con trỏ
        /// </summary>
        private IntPtr ReadPointerAddress(IntPtr address)
        {
            try
            {
                byte[] buffer = new byte[4]; // 4 bytes cho địa chỉ 32-bit
                int bytesRead;

                if (!ReadProcessMemory(processHandle, address, buffer, buffer.Length, out bytesRead))
                {
                    Debug.WriteLine($"Không thể đọc bộ nhớ tại {address.ToString("X")}");
                    return IntPtr.Zero;
                }

                return (IntPtr)BitConverter.ToInt32(buffer, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đọc con trỏ: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Đọc giá trị Int32 từ một địa chỉ cụ thể
        /// </summary>
        public int ReadInt32FromAddress(IntPtr address)
        {
            try
            {
                byte[] buffer = new byte[4]; // 4 bytes cho Int32
                int bytesRead;

                if (!ReadProcessMemory(processHandle, address, buffer, buffer.Length, out bytesRead) || bytesRead != buffer.Length)
                {
                    return 0;
                }

                return BitConverter.ToInt32(buffer, 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đọc Int32: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Quét bộ nhớ để tìm địa chỉ chứa giá trị Vcoin
        /// </summary>
        /// <param name="vcoinValue">Giá trị Vcoin để tìm kiếm</param>
        /// <returns>Danh sách các địa chỉ tiềm năng</returns>
        public List<IntPtr> ScanForVcoinAddresses(int vcoinValue)
        {
            List<IntPtr> results = new List<IntPtr>();

            try
            {
                if (processHandle == IntPtr.Zero || gameProcess == null || !IsProcessRunning())
                {
                    return results;
                }

                if (UpdateStatus != null)
                {
                    UpdateStatus("Bắt đầu quét bộ nhớ...");
                }

                // Quét module chính
                ScanModuleForValue(gameProcess.MainModule, vcoinValue, results);

                // Quét các module khác
                foreach (ProcessModule module in gameProcess.Modules)
                {
                    if (module != gameProcess.MainModule)
                    {
                        ScanModuleForValue(module, vcoinValue, results);
                    }
                }

                if (UpdateStatus != null)
                {
                    UpdateStatus($"Tìm thấy {results.Count} địa chỉ tiềm năng.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi quét bộ nhớ: {ex.Message}");
            }

            return results;
        }

        private void ScanModuleForValue(ProcessModule module, int vcoinValue, List<IntPtr> results)
        {
            try
            {
                // Các thông số quét
                long startAddress = (long)module.BaseAddress;
                long endAddress = startAddress + module.ModuleMemorySize;
                int chunkSize = 4096; // Kích thước mỗi lần đọc

                byte[] valueBytes = BitConverter.GetBytes(vcoinValue);

                for (long currentAddress = startAddress; currentAddress < endAddress; currentAddress += chunkSize)
                {
                    // Báo cáo tiến độ mỗi 1 triệu địa chỉ
                    if (currentAddress % 10000000 == 0)
                    {
                        int progress = (int)((currentAddress - startAddress) * 100 / (endAddress - startAddress));
                        if (UpdateStatus != null)
                        {
                            UpdateStatus($"Đang quét: {currentAddress:X} ({progress}%)");
                        }
                    }

                    try
                    {
                        // Đọc một khối bộ nhớ
                        byte[] buffer = new byte[chunkSize];
                        int bytesRead;

                        if (!ReadProcessMemory((IntPtr)processHandle, (IntPtr)currentAddress, buffer, buffer.Length, out bytesRead))
                        {
                            continue;
                        }

                        // Tìm kiếm giá trị trong khối
                        for (int i = 0; i < bytesRead - 3; i++)
                        {
                            if (buffer[i] == valueBytes[0] &&
                                buffer[i + 1] == valueBytes[1] &&
                                buffer[i + 2] == valueBytes[2] &&
                                buffer[i + 3] == valueBytes[3])
                            {
                                // Tìm thấy giá trị trùng khớp
                                results.Add((IntPtr)(currentAddress + i));

                                // Giới hạn số lượng kết quả để tránh quá tải
                                if (results.Count > 1000)
                                {
                                    return;
                                }
                            }
                        }
                    }
                    catch { } // Bỏ qua lỗi đọc
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi quét module: {ex.Message}");
            }
        }

        /// <summary>
        /// Lưu địa chỉ Vcoin đã tìm được để sử dụng cho lần sau
        /// </summary>
        /// <param name="address">Địa chỉ Vcoin</param>
        /// <param name="fileName">Tên file để lưu (tùy chọn)</param>
        public void SaveVcoinAddress(IntPtr address, string fileName = "vcoin_address.dat")
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName))
                {
                    // Lưu cả địa chỉ và thông tin phiên bản game để kiểm tra sau này
                    writer.WriteLine(address.ToInt64().ToString());
                    if (gameProcess != null)
                    {
                        writer.WriteLine(gameProcess.MainModule.FileName);
                        writer.WriteLine(gameProcess.MainModule.FileVersionInfo.FileVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi khi lưu địa chỉ Vcoin: " + ex.Message);
            }
        }

        /// <summary>
        /// Tải địa chỉ Vcoin đã lưu trước đó
        /// </summary>
        /// <param name="fileName">Tên file để tải</param>
        /// <returns>Địa chỉ đã lưu, hoặc IntPtr.Zero nếu không tìm thấy</returns>
        public IntPtr LoadVcoinAddress(string fileName = "vcoin_address.dat")
        {
            try
            {
                if (File.Exists(fileName))
                {
                    string[] lines = File.ReadAllLines(fileName);
                    if (lines.Length > 0)
                    {
                        long addressValue;
                        if (long.TryParse(lines[0], out addressValue))
                        {
                            // Kiểm tra phiên bản game nếu có
                            if (lines.Length > 2 && gameProcess != null)
                            {
                                // Đường dẫn file và phiên bản phải khớp để đảm bảo địa chỉ còn hợp lệ
                                string savedPath = lines[1];
                                string savedVersion = lines[2];
                                string currentPath = gameProcess.MainModule.FileName;
                                string currentVersion = gameProcess.MainModule.FileVersionInfo.FileVersion;

                                if (savedPath != currentPath || savedVersion != currentVersion)
                                {
                                    // Khác phiên bản, không sử dụng địa chỉ này
                                    return IntPtr.Zero;
                                }
                            }

                            return (IntPtr)addressValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Lỗi khi tải địa chỉ Vcoin: " + ex.Message);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Tìm kiếm thông minh địa chỉ Vcoin, quét nhiều giá trị
        /// </summary>
        public List<IntPtr> SmartScanForVcoin(int currentVcoin)
        {
            List<IntPtr> results = new List<IntPtr>();

            try
            {
                // Quét lần đầu tìm các địa chỉ có giá trị bằng Vcoin hiện tại
                List<IntPtr> initialMatches = ScanForVcoinAddresses(currentVcoin);

                if (initialMatches.Count == 0 || initialMatches.Count > 10000)
                {
                    // Nếu kết quả quá nhiều hoặc không có, thì không cần tiếp tục
                    return initialMatches;
                }

                // Lưu trữ các giá trị ban đầu
                Dictionary<IntPtr, int> initialValues = new Dictionary<IntPtr, int>();
                foreach (IntPtr addr in initialMatches)
                {
                    initialValues[addr] = ReadInt32FromAddress(addr);
                }

                // Thông báo cho người dùng biết quá trình quét cần thời gian
                if (UpdateStatus != null)
                {
                    UpdateStatus($"Đã tìm thấy {initialMatches.Count} địa chỉ ban đầu. Làm ván tiếp theo để lọc...");
                }

                // Đợi người dùng hoàn thành thêm một ván đấu
                // (Chúng ta mong đợi giá trị Vcoin sẽ thay đổi)

                // Quét lại, chỉ giữ lại các địa chỉ thay đổi một cách hợp lý
                foreach (IntPtr addr in initialMatches)
                {
                    int newValue = ReadInt32FromAddress(addr);
                    int oldValue = initialValues[addr];

                    // Nếu giá trị tăng lên và trong phạm vi hợp lý của Vcoin tăng sau một ván
                    if (newValue > oldValue && (newValue - oldValue) < 1000)
                    {
                        results.Add(addr);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi quét thông minh: {ex.Message}");
            }

            return results;
        }
    }
}