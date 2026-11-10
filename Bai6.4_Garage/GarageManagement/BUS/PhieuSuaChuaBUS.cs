using System;
using System.Data;
using System.Linq;
using System.Text;
using GarageManagement.DAL;
using GarageManagement.DTO;

namespace GarageManagement.BUS
{
    public class PhieuSuaChuaBUS
    {
        private readonly PhieuSuaChuaDAL dal = new PhieuSuaChuaDAL();

        public DataTable GetAll() => dal.GetAll();
        public DataTable GetChiTiet(int maPhieu) => dal.GetChiTiet(maPhieu);

        /// <summary>Tổng chi phí = Tổng (Số lượng phụ tùng × Đơn giá) + Tổng tiền công.</summary>
        public static decimal TinhTongTien(PhieuSuaChuaDTO p) => p.ChiTiet.Sum(c => c.SoLuong * c.DonGia + c.TienCong);

        public int LapPhieu(PhieuSuaChuaDTO p)
        {
            if (string.IsNullOrWhiteSpace(p.BienSo)) throw new ArgumentException("Vui lòng chọn xe.");
            if (p.ChiTiet.Count == 0) throw new ArgumentException("Phiếu phải có ít nhất 1 phụ tùng / dịch vụ.");
            if (p.ChiTiet.Any(c => c.SoLuong <= 0)) throw new ArgumentException("Số lượng phải > 0.");
            p.TongTien = TinhTongTien(p);
            return dal.LapPhieu(p);
        }

        public void HoanThanh(int maPhieu, string trangThaiHienTai)
        {
            if (trangThaiHienTai != "Đang sửa") throw new InvalidOperationException("Chỉ phiếu 'Đang sửa' mới chuyển sang 'Đã hoàn thành'.");
            dal.UpdateTrangThai(maPhieu, "Đã hoàn thành");
        }

        public string ThanhToan(int maPhieu, string trangThaiHienTai)
        {
            if (trangThaiHienTai == "Đã thanh toán") throw new InvalidOperationException("Phiếu đã được thanh toán.");
            dal.UpdateTrangThai(maPhieu, "Đã thanh toán");
            return XuatHoaDon(maPhieu);
        }

        public string XuatHoaDon(int maPhieu)
        {
            DataRow p = dal.GetAll().AsEnumerable().First(r => r.Field<int>("MaPhieu") == maPhieu);
            DataTable ct = dal.GetChiTiet(maPhieu);
            var sb = new StringBuilder();
            sb.AppendLine("========== HÓA ĐƠN DỊCH VỤ GARAGE ==========");
            sb.AppendLine($"Số phiếu: {maPhieu}      Ngày: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Khách hàng: {p["TenKH"]}    Biển số: {p["BienSo"]}");
            sb.AppendLine($"Nội dung: {p["NoiDungSua"]}");
            sb.AppendLine("--------------------------------------------");
            foreach (DataRow r in ct.Rows)
                sb.AppendLine($"- {r["TenPhuTung"]} x{r["SoLuong"]} @ {r["DonGia"]:N0} + công {r["TienCong"]:N0} = {r["ThanhTien"]:N0}");
            sb.AppendLine("--------------------------------------------");
            sb.AppendLine($"TỔNG CỘNG: {p["TongTien"]:N0} VNĐ");
            return sb.ToString();
        }

        public DataTable DoanhThuTheoThang() => dal.DoanhThuTheoThang();
        public DataTable PhuTungSapHet(int nguong = 5) => dal.PhuTungSapHet(nguong);
    }
}
