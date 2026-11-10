using System;

namespace KaraokeManagement.DTO
{
    public class HoaDonDTO
    {
        public int MaHD { get; set; }
        public int MaPhong { get; set; }
        public string TenPhong { get; set; }
        public decimal GiaTheoGio { get; set; }
        public DateTime GioVao { get; set; }
        public DateTime? GioRa { get; set; }
        public decimal TongTienPhong { get; set; }
        public decimal TongTienDichVu { get; set; }
        public bool TrangThai { get; set; }
        public decimal TongCong => TongTienPhong + TongTienDichVu;
    }
}
