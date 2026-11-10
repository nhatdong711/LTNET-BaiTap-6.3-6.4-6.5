using System;
using System.Data;
using Microsoft.Data.SqlClient;
using GarageManagement.DTO;

namespace GarageManagement.DAL
{
    public class XeDAL
    {
        /// <summary>Tra cứu xe theo biển số bằng SqlParameter (chống SQL Injection).</summary>
        public DataTable TraCuu(string bienSo) => DBConnect.ExecuteQuery(
            @"SELECT x.BienSo, x.HieuXe, x.NamSanXuat, kh.MaKH, kh.TenKH, kh.DienThoai, kh.DiaChi
              FROM Xe x JOIN KhachHang kh ON x.MaKH = kh.MaKH
              WHERE x.BienSo LIKE @bs ORDER BY x.BienSo",
            new SqlParameter("@bs", SqlDbType.VarChar, 22) { Value = "%" + bienSo + "%" });

        public bool Exists(string bienSo) =>
            Convert.ToInt32(DBConnect.ExecuteScalar("SELECT COUNT(*) FROM Xe WHERE BienSo=@bs", new SqlParameter("@bs", bienSo))) > 0;

        public int FindKhachHangByPhone(string phone)
        {
            object r = DBConnect.ExecuteScalar("SELECT MaKH FROM KhachHang WHERE DienThoai=@dt", new SqlParameter("@dt", phone));
            return r == null ? 0 : Convert.ToInt32(r);
        }

        /// <summary>Thêm khách hàng (nếu chưa có theo SĐT) và xe mới trong 1 transaction.</summary>
        public void TiepNhanXeMoi(KhachHangDTO kh, XeDTO xe)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var find = new SqlCommand("SELECT MaKH FROM KhachHang WHERE DienThoai=@dt", conn, tran);
                        find.Parameters.AddWithValue("@dt", kh.DienThoai);
                        object found = find.ExecuteScalar();
                        int maKH;
                        if (found != null) maKH = Convert.ToInt32(found);
                        else
                        {
                            var ins = new SqlCommand("INSERT INTO KhachHang(TenKH, DienThoai, DiaChi) VALUES(@ten,@dt,@dc); SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tran);
                            ins.Parameters.AddWithValue("@ten", kh.TenKH);
                            ins.Parameters.AddWithValue("@dt", kh.DienThoai);
                            ins.Parameters.AddWithValue("@dc", (object)kh.DiaChi ?? DBNull.Value);
                            maKH = Convert.ToInt32(ins.ExecuteScalar());
                        }
                        var insXe = new SqlCommand("INSERT INTO Xe(BienSo, MaKH, HieuXe, NamSanXuat) VALUES(@bs,@kh,@hx,@nam)", conn, tran);
                        insXe.Parameters.AddWithValue("@bs", xe.BienSo);
                        insXe.Parameters.AddWithValue("@kh", maKH);
                        insXe.Parameters.AddWithValue("@hx", (object)xe.HieuXe ?? DBNull.Value);
                        insXe.Parameters.AddWithValue("@nam", (object)xe.NamSanXuat ?? DBNull.Value);
                        insXe.ExecuteNonQuery();
                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool Update(KhachHangDTO kh, XeDTO xe)
        {
            DBConnect.ExecuteNonQuery("UPDATE KhachHang SET TenKH=@ten, DienThoai=@dt, DiaChi=@dc WHERE MaKH=@ma",
                new SqlParameter("@ten", kh.TenKH), new SqlParameter("@dt", kh.DienThoai),
                new SqlParameter("@dc", (object)kh.DiaChi ?? DBNull.Value), new SqlParameter("@ma", kh.MaKH));
            return DBConnect.ExecuteNonQuery("UPDATE Xe SET HieuXe=@hx, NamSanXuat=@nam WHERE BienSo=@bs",
                new SqlParameter("@hx", (object)xe.HieuXe ?? DBNull.Value),
                new SqlParameter("@nam", (object)xe.NamSanXuat ?? DBNull.Value),
                new SqlParameter("@bs", xe.BienSo)) > 0;
        }

        public bool Delete(string bienSo)
        {
            int n = Convert.ToInt32(DBConnect.ExecuteScalar("SELECT COUNT(*) FROM PhieuSuaChua WHERE BienSo=@bs", new SqlParameter("@bs", bienSo)));
            if (n > 0) throw new InvalidOperationException("Xe đã có phiếu sửa chữa, không thể xóa.");
            return DBConnect.ExecuteNonQuery("DELETE FROM Xe WHERE BienSo=@bs", new SqlParameter("@bs", bienSo)) > 0;
        }
    }
}
