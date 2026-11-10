using System;
using System.Collections.Generic;

namespace GarageManagement.DTO
{
    public class PhieuSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public string BienSo { get; set; }
        public DateTime NgayNhan { get; set; }
        public string NoiDungSua { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public List<ChiTietSuaChuaDTO> ChiTiet { get; set; } = new List<ChiTietSuaChuaDTO>();
    }

    public class ChiTietSuaChuaDTO
    {
        public int MaPhieu { get; set; }
        public int MaPhuTung { get; set; }
        public string TenPhuTung { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal TienCong { get; set; }
        public decimal ThanhTien => SoLuong * DonGia + TienCong;
    }
}
