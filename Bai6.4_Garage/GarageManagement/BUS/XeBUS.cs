using System;
using System.Data;
using System.Text.RegularExpressions;
using GarageManagement.DAL;
using GarageManagement.DTO;

namespace GarageManagement.BUS
{
    public class XeBUS
    {
        private readonly XeDAL dal = new XeDAL();

        public DataTable TraCuu(string bienSo) => dal.TraCuu((bienSo ?? "").Trim().ToUpper());

        private static void Validate(KhachHangDTO kh, XeDTO xe)
        {
            if (string.IsNullOrWhiteSpace(xe.BienSo)) throw new ArgumentException("Biển số không được để trống.");
            if (string.IsNullOrWhiteSpace(kh.TenKH)) throw new ArgumentException("Tên chủ xe không được để trống.");
            if (!Regex.IsMatch(kh.DienThoai ?? "", @"^0\d{9,10}$")) throw new ArgumentException("Số điện thoại không hợp lệ (10-11 số, bắt đầu bằng 0).");
            if (xe.NamSanXuat.HasValue && (xe.NamSanXuat < 1950 || xe.NamSanXuat > DateTime.Now.Year))
                throw new ArgumentException("Năm sản xuất không hợp lệ.");
        }

        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            xe.BienSo = xe.BienSo?.Trim().ToUpper();
            Validate(kh, xe);
            if (dal.Exists(xe.BienSo)) throw new InvalidOperationException("Biển số đã tồn tại trong hệ thống.");
            dal.TiepNhanXeMoi(kh, xe);
        }

        public bool Update(KhachHangDTO kh, XeDTO xe) { Validate(kh, xe); return dal.Update(kh, xe); }
        public bool Delete(string bienSo) => dal.Delete(bienSo);
    }
}
