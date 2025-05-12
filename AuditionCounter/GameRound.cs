using System;

namespace AuditionCounter
{
    public class GameRound
    {
        /// <summary>
        /// Số thứ tự của ván đấu
        /// </summary>
        public int RoundNumber { get; set; }

        /// <summary>
        /// Số VCoin nhận được từ ván đấu này
        /// </summary>
        public int VcoinEarned { get; set; }

        /// <summary>
        /// Thời điểm hoàn thành ván đấu
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Ghi chú cho ván đấu (tùy chọn)
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Chuyển đổi thành chuỗi hiển thị trong ListView
        /// </summary>
        /// <returns>Chuỗi định dạng hiển thị</returns>
        public override string ToString()
        {
            // Định dạng: "Ván 1: +100vc (12:30:45)"
            return $"Ván {RoundNumber}: +{VcoinEarned}vc ({Timestamp.ToString("HH:mm:ss")})";
        }
    }
}