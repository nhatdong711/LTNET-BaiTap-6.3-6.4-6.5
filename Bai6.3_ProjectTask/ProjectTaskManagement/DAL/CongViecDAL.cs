using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.DAL
{
    public class CongViecDAL
    {
        private const string SelectSql =
            @"SELECT cv.MaTask, cv.MaDuAn, da.TenDuAn, cv.TenTask, cv.MoTa, cv.HanNop, cv.DoUuTien, cv.TrangThai,
                     ISNULL(STUFF((SELECT ', ' + tv.HoTen FROM PhanCong pc JOIN ThanhVien tv ON pc.MaThanhVien = tv.MaThanhVien
                                   WHERE pc.MaTask = cv.MaTask FOR XML PATH('')), 1, 2, ''), '') AS NguoiThucHien
              FROM CongViec cv JOIN DuAn da ON cv.MaDuAn = da.MaDuAn";

        /// <summary>Lọc công việc theo dự án và trạng thái (Parameterized SQL). maDuAn = 0 / trangThai = null: lấy tất cả.</summary>
        public DataTable Filter(int maDuAn, string trangThai)
        {
            string sql = SelectSql + " WHERE (@da = 0 OR cv.MaDuAn = @da) AND (@tt IS NULL OR cv.TrangThai = @tt) ORDER BY cv.MaTask";
            return DBConnect.ExecuteQuery(sql,
                new SqlParameter("@da", maDuAn),
                new SqlParameter("@tt", SqlDbType.NVarChar, 50) { Value = (object)trangThai ?? DBNull.Value });
        }

        public int Insert(CongViecDTO c)
        {
            object id = DBConnect.ExecuteScalar(
                @"INSERT INTO CongViec(MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai)
                  VALUES(@da, @ten, @mota, @han, @ut, @tt); SELECT CAST(SCOPE_IDENTITY() AS int);",
                Params(c));
            return Convert.ToInt32(id);
        }

        public bool Update(CongViecDTO c)
        {
            var prms = new List<SqlParameter>(Params(c)) { new SqlParameter("@ma", c.MaTask) };
            return DBConnect.ExecuteNonQuery(
                @"UPDATE CongViec SET MaDuAn=@da, TenTask=@ten, MoTa=@mota, HanNop=@han, DoUuTien=@ut, TrangThai=@tt
                  WHERE MaTask=@ma", prms.ToArray()) > 0;
        }

        /// <summary>Chuyển trạng thái công việc (VD: In Progress -> Done).</summary>
        public bool UpdateTrangThai(int maTask, string trangThai) => DBConnect.ExecuteNonQuery(
            "UPDATE CongViec SET TrangThai=@tt WHERE MaTask=@ma",
            new SqlParameter("@tt", trangThai), new SqlParameter("@ma", maTask)) > 0;

        public bool Delete(int maTask)
        {
            DBConnect.ExecuteNonQuery("DELETE FROM PhanCong WHERE MaTask=@ma", new SqlParameter("@ma", maTask));
            return DBConnect.ExecuteNonQuery("DELETE FROM CongViec WHERE MaTask=@ma", new SqlParameter("@ma", maTask)) > 0;
        }

        private static SqlParameter[] Params(CongViecDTO c) => new[]
        {
            new SqlParameter("@da", c.MaDuAn),
            new SqlParameter("@ten", c.TenTask),
            new SqlParameter("@mota", (object)c.MoTa ?? DBNull.Value),
            new SqlParameter("@han", (object)c.HanNop ?? DBNull.Value),
            new SqlParameter("@ut", (object)c.DoUuTien ?? DBNull.Value),
            new SqlParameter("@tt", c.TrangThai)
        };
    }
}
