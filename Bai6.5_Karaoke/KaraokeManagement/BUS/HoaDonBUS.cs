using System;
using System.Data;
using System.Linq;
using System.Text;
using KaraokeManagement.DAL;
using KaraokeManagement.DTO;

namespace KaraokeManagement.BUS
{
    public class HoaDonBUS
    {
        private readonly HoaDonDAL dal = new HoaDonDAL();

        public HoaDonDTO GetHoaDonDangMo(int maPhong) => dal.GetHoaDonDangMo(maPhong);
        public DataTable GetChiTietDichVu(int maHD) => dal.GetChiTietDichVu(maHD);

        public void ThemDichVu(int maHD, DataRow dv, int soLuong)
        {
            if (soLuong <= 0) throw new ArgumentException("Số lượng phải > 0.");
            int ton = Convert.ToInt32(dv["TonKho"]);
            if (soLuong > ton) throw new InvalidOperationException($"\"{dv["TenDV"]}\" chỉ còn {ton} trong kho.");
            dal.ThemDichVu(maHD, Convert.ToInt32(dv["MaDV"]), soLuong, Convert.ToDecimal(dv["DonGia"]));
        }

        public void XoaDichVu(int maHD, int maDV) => dal.XoaDichVu(maHD, maDV);

        /// <summary>Tính tiền: thời gian hát = GioRa - GioVao; TongTienPhong = (Số giờ) * GiaTheoGio (làm tròn lên mỗi 15 phút, tối thiểu 1 giờ).</summary>
        public HoaDonDTO TinhTien(HoaDonDTO hd, DateTime gioRa)
        {
            hd.GioRa = gioRa;
            TimeSpan t = gioRa - hd.GioVao;
            double phut = Math.Ceiling(Math.Max(t.TotalMinutes, 60) / 15.0) * 15;
            decimal soGio = (decimal)phut / 60m;
            hd.TongTienPhong = Math.Round(soGio * hd.GiaTheoGio, 0);
            hd.TongTienDichVu = GetChiTietDichVu(hd.MaHD).AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien"));
            return hd;
        }

        public string ThanhToan(HoaDonDTO hd)
        {
            TinhTien(hd, DateTime.Now);
            dal.ThanhToan(hd);
            return InHoaDon(hd);
        }

        public string InHoaDon(HoaDonDTO hd)
        {
            TimeSpan t = hd.GioRa.Value - hd.GioVao;
            var sb = new StringBuilder();
            sb.AppendLine("========= HÓA ĐƠN KARAOKE =========");
            sb.AppendLine($"Hóa đơn #{hd.MaHD} - {hd.TenPhong}");
            sb.AppendLine($"Giờ vào: {hd.GioVao:HH:mm dd/MM}   Giờ ra: {hd.GioRa:HH:mm dd/MM}");
            sb.AppendLine($"Thời gian hát: {(int)t.TotalHours} giờ {t.Minutes} phút");
            sb.AppendLine($"Tiền phòng ({hd.GiaTheoGio:N0}/giờ): {hd.TongTienPhong:N0}");
            sb.AppendLine("-----------------------------------");
            foreach (DataRow r in GetChiTietDichVu(hd.MaHD).Rows)
                sb.AppendLine($"  {r["TenDV"]} x{r["SoLuong"]} = {r["ThanhTien"]:N0}");
            sb.AppendLine($"Tiền dịch vụ: {hd.TongTienDichVu:N0}");
            sb.AppendLine("-----------------------------------");
            sb.AppendLine($"TỔNG THANH TOÁN: {hd.TongCong:N0} VNĐ");
            return sb.ToString();
        }

        public DataTable DoanhThuTheoNgay() => dal.DoanhThuTheoNgay();
        public DataTable LichSuHoaDon() => dal.LichSuHoaDon();
    }
}
