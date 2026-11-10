using System;
using System.Data;
using Microsoft.Data.SqlClient;
using KaraokeManagement.DTO;

namespace KaraokeManagement.DAL
{
    public class HoaDonDAL
    {
        public HoaDonDTO GetHoaDonDangMo(int maPhong)
        {
            const string sql = @"SELECT TOP 1 hd.MaHD, hd.MaPhong, p.TenPhong, p.GiaTheoGio, hd.GioVao
                                 FROM HoaDon hd JOIN PhongHat p ON hd.MaPhong = p.MaPhong
                                 WHERE hd.MaPhong = @p AND hd.TrangThai = 0 ORDER BY hd.MaHD DESC";
            using (var conn = DBConnect.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@p", maPhong);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    return new HoaDonDTO
                    {
                        MaHD = rd.GetInt32(0), MaPhong = rd.GetInt32(1), TenPhong = rd.GetString(2),
                        GiaTheoGio = rd.GetDecimal(3), GioVao = rd.GetDateTime(4)
                    };
                }
            }
        }

        /// <summary>DataTable chi tiết dịch vụ của hóa đơn (dùng SqlDataAdapter - mô hình ngắt kết nối).</summary>
        public DataTable GetChiTietDichVu(int maHD)
        {
            using (var adapter = new SqlDataAdapter(
                @"SELECT ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia, (ct.SoLuong * ct.DonGia) AS ThanhTien
                  FROM ChiTietDichVu ct JOIN DichVu dv ON ct.MaDV = dv.MaDV WHERE ct.MaHD = @hd", DBConnect.GetConnection()))
            {
                adapter.SelectCommand.Parameters.AddWithValue("@hd", maHD);
                var dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
        }

        /// <summary>Thêm món vào phòng (cộng dồn nếu món đã có).</summary>
        public void ThemDichVu(int maHD, int maDV, int soLuong, decimal donGia)
        {
            DBConnect.ExecuteNonQuery(
                @"IF EXISTS (SELECT 1 FROM ChiTietDichVu WHERE MaHD=@hd AND MaDV=@dv)
                      UPDATE ChiTietDichVu SET SoLuong = SoLuong + @sl WHERE MaHD=@hd AND MaDV=@dv
                  ELSE
                      INSERT INTO ChiTietDichVu(MaHD, MaDV, SoLuong, DonGia) VALUES(@hd, @dv, @sl, @gia)",
                new SqlParameter("@hd", maHD), new SqlParameter("@dv", maDV),
                new SqlParameter("@sl", soLuong), new SqlParameter("@gia", donGia));
        }

        public void XoaDichVu(int maHD, int maDV) => DBConnect.ExecuteNonQuery(
            "DELETE FROM ChiTietDichVu WHERE MaHD=@hd AND MaDV=@dv", new SqlParameter("@hd", maHD), new SqlParameter("@dv", maDV));

        /// <summary>
        /// Thanh toán bằng SqlTransaction:
        /// 1. Cập nhật GioRa, TongTienPhong, TongTienDichVu, TrangThai = 1 cho HoaDon
        /// 2. Trừ TonKho tương ứng trong bảng DichVu (thiếu hàng -> Rollback)
        /// 3. Cập nhật trạng thái PhongHat về "Trống"
        /// 4. trans.Commit()
        /// </summary>
        public void ThanhToan(HoaDonDTO hd)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    var cmd1 = new SqlCommand(
                        @"UPDATE HoaDon SET GioRa=@ra, TongTienPhong=@tp, TongTienDichVu=@tdv, TrangThai=1
                          WHERE MaHD=@hd AND TrangThai=0", conn, trans);
                    cmd1.Parameters.AddWithValue("@ra", hd.GioRa.Value);
                    cmd1.Parameters.AddWithValue("@tp", hd.TongTienPhong);
                    cmd1.Parameters.AddWithValue("@tdv", hd.TongTienDichVu);
                    cmd1.Parameters.AddWithValue("@hd", hd.MaHD);
                    if (cmd1.ExecuteNonQuery() == 0) throw new InvalidOperationException("Hóa đơn không tồn tại hoặc đã thanh toán.");

                    // Kiểm tra tồn kho trước khi trừ
                    var check = new SqlCommand(
                        @"SELECT TOP 1 dv.TenDV FROM ChiTietDichVu ct JOIN DichVu dv ON ct.MaDV = dv.MaDV
                          WHERE ct.MaHD = @hd AND dv.TonKho < ct.SoLuong", conn, trans);
                    check.Parameters.AddWithValue("@hd", hd.MaHD);
                    object thieu = check.ExecuteScalar();
                    if (thieu != null) throw new InvalidOperationException($"Dịch vụ \"{thieu}\" không đủ tồn kho!");

                    var cmd2 = new SqlCommand(
                        @"UPDATE dv SET dv.TonKho = dv.TonKho - ct.SoLuong
                          FROM DichVu dv JOIN ChiTietDichVu ct ON dv.MaDV = ct.MaDV WHERE ct.MaHD = @hd", conn, trans);
                    cmd2.Parameters.AddWithValue("@hd", hd.MaHD);
                    cmd2.ExecuteNonQuery();

                    var cmd3 = new SqlCommand("UPDATE PhongHat SET TrangThai = N'Trống' WHERE MaPhong=@p", conn, trans);
                    cmd3.Parameters.AddWithValue("@p", hd.MaPhong);
                    cmd3.ExecuteNonQuery();

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public DataTable DoanhThuTheoNgay() => DBConnect.ExecuteQuery(
            @"SELECT CONVERT(date, GioRa) AS Ngay, COUNT(*) AS SoHoaDon, SUM(TongTienPhong) AS TienPhong,
                     SUM(TongTienDichVu) AS TienDichVu, SUM(TongTienPhong + TongTienDichVu) AS TongDoanhThu
              FROM HoaDon WHERE TrangThai = 1 GROUP BY CONVERT(date, GioRa) ORDER BY Ngay DESC");

        public DataTable LichSuHoaDon() => DBConnect.ExecuteQuery(
            @"SELECT hd.MaHD, p.TenPhong, hd.GioVao, hd.GioRa, hd.TongTienPhong, hd.TongTienDichVu,
                     (hd.TongTienPhong + hd.TongTienDichVu) AS TongCong,
                     CASE hd.TrangThai WHEN 1 THEN N'Đã thanh toán' ELSE N'Chưa thanh toán' END AS TrangThai
              FROM HoaDon hd JOIN PhongHat p ON hd.MaPhong = p.MaPhong ORDER BY hd.MaHD DESC");
    }
}
