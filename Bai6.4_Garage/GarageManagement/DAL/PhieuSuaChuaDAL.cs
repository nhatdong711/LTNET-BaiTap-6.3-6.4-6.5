using System;
using System.Data;
using Microsoft.Data.SqlClient;
using GarageManagement.DTO;

namespace GarageManagement.DAL
{
    public class PhieuSuaChuaDAL
    {
        public DataTable GetAll() => DBConnect.ExecuteQuery(
            @"SELECT p.MaPhieu, p.BienSo, kh.TenKH, p.NgayNhan, p.NoiDungSua, p.TongTien, p.TrangThai
              FROM PhieuSuaChua p JOIN Xe x ON p.BienSo = x.BienSo JOIN KhachHang kh ON x.MaKH = kh.MaKH
              ORDER BY p.MaPhieu DESC");

        public DataTable GetChiTiet(int maPhieu) => DBConnect.ExecuteQuery(
            @"SELECT ct.MaPhuTung, pt.TenPhuTung, ct.SoLuong, pt.DonGia, ct.TienCong, (ct.SoLuong * pt.DonGia + ct.TienCong) AS ThanhTien
              FROM ChiTietSuaChua ct JOIN PhuTung pt ON ct.MaPhuTung = pt.MaPhuTung WHERE ct.MaPhieu = @ma",
            new SqlParameter("@ma", maPhieu));

        /// <summary>
        /// Lập phiếu sửa chữa bằng SqlTransaction:
        /// 1. Tạo PhieuSuaChua  2. Tạo ChiTietSuaChua  3. Trừ tồn kho PhuTung
        /// 4. Nếu phụ tùng không đủ số lượng -> Rollback và báo lỗi.
        /// </summary>
        public int LapPhieu(PhieuSuaChuaDTO phieu)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    var cmdPhieu = new SqlCommand(
                        @"INSERT INTO PhieuSuaChua(BienSo, NgayNhan, NoiDungSua, TongTien, TrangThai)
                          VALUES(@bs, GETDATE(), @nd, @tong, N'Đang sửa'); SELECT CAST(SCOPE_IDENTITY() AS int);", conn, trans);
                    cmdPhieu.Parameters.AddWithValue("@bs", phieu.BienSo);
                    cmdPhieu.Parameters.AddWithValue("@nd", (object)phieu.NoiDungSua ?? DBNull.Value);
                    cmdPhieu.Parameters.AddWithValue("@tong", phieu.TongTien);
                    int maPhieu = Convert.ToInt32(cmdPhieu.ExecuteScalar());

                    foreach (var ct in phieu.ChiTiet)
                    {
                        var cmdCT = new SqlCommand(
                            "INSERT INTO ChiTietSuaChua(MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES(@mp, @pt, @sl, @tc)", conn, trans);
                        cmdCT.Parameters.AddWithValue("@mp", maPhieu);
                        cmdCT.Parameters.AddWithValue("@pt", ct.MaPhuTung);
                        cmdCT.Parameters.AddWithValue("@sl", ct.SoLuong);
                        cmdCT.Parameters.AddWithValue("@tc", ct.TienCong);
                        cmdCT.ExecuteNonQuery();

                        // Trừ kho: chỉ trừ khi còn đủ hàng
                        var cmdKho = new SqlCommand(
                            "UPDATE PhuTung SET SoLuongTon = SoLuongTon - @sl WHERE MaPhuTung = @pt AND SoLuongTon >= @sl", conn, trans);
                        cmdKho.Parameters.AddWithValue("@sl", ct.SoLuong);
                        cmdKho.Parameters.AddWithValue("@pt", ct.MaPhuTung);
                        if (cmdKho.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException($"Phụ tùng \"{ct.TenPhuTung}\" không đủ số lượng trong kho!");
                    }

                    trans.Commit();
                    return maPhieu;
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }

        public bool UpdateTrangThai(int maPhieu, string trangThai) => DBConnect.ExecuteNonQuery(
            "UPDATE PhieuSuaChua SET TrangThai=@tt WHERE MaPhieu=@ma",
            new SqlParameter("@tt", trangThai), new SqlParameter("@ma", maPhieu)) > 0;

        public DataTable DoanhThuTheoThang() => DBConnect.ExecuteQuery(
            @"SELECT FORMAT(NgayNhan, 'MM/yyyy') AS Thang, COUNT(*) AS SoPhieu, SUM(TongTien) AS DoanhThu
              FROM PhieuSuaChua WHERE TrangThai = N'Đã thanh toán'
              GROUP BY FORMAT(NgayNhan, 'MM/yyyy') ORDER BY MIN(NgayNhan)");

        public DataTable PhuTungSapHet(int nguong) => DBConnect.ExecuteQuery(
            "SELECT MaPhuTung, TenPhuTung, SoLuongTon FROM PhuTung WHERE SoLuongTon <= @n ORDER BY SoLuongTon",
            new SqlParameter("@n", nguong));
    }
}
