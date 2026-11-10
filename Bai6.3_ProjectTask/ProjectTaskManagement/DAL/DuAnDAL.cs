using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.DAL
{
    public class DuAnDAL
    {
        public DataTable GetAll() =>
            DBConnect.ExecuteQuery("SELECT MaDuAn, TenDuAn, NgayBatDau, NgayKetThuc, TrangThai FROM DuAn ORDER BY MaDuAn");

        public bool Insert(DuAnDTO d) => DBConnect.ExecuteNonQuery(
            "INSERT INTO DuAn(TenDuAn, NgayBatDau, NgayKetThuc, TrangThai) VALUES(@ten, @bd, @kt, @tt)",
            new SqlParameter("@ten", d.TenDuAn),
            new SqlParameter("@bd", (object)d.NgayBatDau ?? DBNull.Value),
            new SqlParameter("@kt", (object)d.NgayKetThuc ?? DBNull.Value),
            new SqlParameter("@tt", d.TrangThai)) > 0;

        public bool Update(DuAnDTO d) => DBConnect.ExecuteNonQuery(
            "UPDATE DuAn SET TenDuAn=@ten, NgayBatDau=@bd, NgayKetThuc=@kt, TrangThai=@tt WHERE MaDuAn=@ma",
            new SqlParameter("@ten", d.TenDuAn),
            new SqlParameter("@bd", (object)d.NgayBatDau ?? DBNull.Value),
            new SqlParameter("@kt", (object)d.NgayKetThuc ?? DBNull.Value),
            new SqlParameter("@tt", d.TrangThai),
            new SqlParameter("@ma", d.MaDuAn)) > 0;

        /// <summary>Xóa dự án kèm toàn bộ công việc + phân công con (trong 1 transaction).</summary>
        public bool Delete(int maDuAn)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var p = new SqlParameter("@ma", maDuAn);
                        new SqlCommand("DELETE pc FROM PhanCong pc JOIN CongViec cv ON pc.MaTask = cv.MaTask WHERE cv.MaDuAn=@ma", conn, tran) { Parameters = { new SqlParameter("@ma", maDuAn) } }.ExecuteNonQuery();
                        new SqlCommand("DELETE FROM CongViec WHERE MaDuAn=@ma", conn, tran) { Parameters = { new SqlParameter("@ma", maDuAn) } }.ExecuteNonQuery();
                        int n = new SqlCommand("DELETE FROM DuAn WHERE MaDuAn=@ma", conn, tran) { Parameters = { p } }.ExecuteNonQuery();
                        tran.Commit();
                        return n > 0;
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
