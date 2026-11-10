using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ProjectTaskManagement.DAL
{
    public class BaoCaoDAL
    {
        /// <summary>Dùng SqlDataReader lấy các công việc quá hạn (HanNop &lt; hôm nay và chưa Done).</summary>
        public DataTable GetCongViecTreHan()
        {
            var dt = new DataTable();
            dt.Columns.Add("MaTask", typeof(int));
            dt.Columns.Add("TenDuAn", typeof(string));
            dt.Columns.Add("TenTask", typeof(string));
            dt.Columns.Add("HanNop", typeof(DateTime));
            dt.Columns.Add("SoNgayTre", typeof(int));
            dt.Columns.Add("TrangThai", typeof(string));

            const string sql = @"SELECT cv.MaTask, da.TenDuAn, cv.TenTask, cv.HanNop, DATEDIFF(day, cv.HanNop, GETDATE()) AS SoNgayTre, cv.TrangThai
                                 FROM CongViec cv JOIN DuAn da ON cv.MaDuAn = da.MaDuAn
                                 WHERE cv.HanNop < GETDATE() AND cv.TrangThai <> @done
                                 ORDER BY cv.HanNop";
            using (var conn = DBConnect.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@done", "Done");
                conn.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        dt.Rows.Add(rd.GetInt32(0), rd.GetString(1), rd.GetString(2), rd.GetDateTime(3), rd.GetInt32(4), rd.GetString(5));
                    }
                }
            }
            return dt;
        }

        /// <summary>Thống kê % hoàn thành theo dự án bằng GROUP BY.</summary>
        public DataTable GetTienDoDuAn() => DBConnect.ExecuteQuery(
            @"SELECT da.MaDuAn, da.TenDuAn,
                     COUNT(cv.MaTask) AS TongTask,
                     SUM(CASE WHEN cv.TrangThai = 'Done' THEN 1 ELSE 0 END) AS DaXong,
                     CAST(CASE WHEN COUNT(cv.MaTask) = 0 THEN 0
                          ELSE 100.0 * SUM(CASE WHEN cv.TrangThai = 'Done' THEN 1 ELSE 0 END) / COUNT(cv.MaTask) END AS decimal(5,2)) AS PhanTramHoanThanh
              FROM DuAn da LEFT JOIN CongViec cv ON da.MaDuAn = cv.MaDuAn
              GROUP BY da.MaDuAn, da.TenDuAn
              ORDER BY da.MaDuAn");

        /// <summary>% hoàn thành của 1 dự án bằng ExecuteScalar.</summary>
        public decimal GetPhanTramHoanThanh(int maDuAn)
        {
            object r = DBConnect.ExecuteScalar(
                @"SELECT CASE WHEN COUNT(*) = 0 THEN 0
                         ELSE 100.0 * SUM(CASE WHEN TrangThai = 'Done' THEN 1 ELSE 0 END) / COUNT(*) END
                  FROM CongViec WHERE MaDuAn = @da", new SqlParameter("@da", maDuAn));
            return r == null || r == DBNull.Value ? 0 : Math.Round(Convert.ToDecimal(r), 2);
        }
    }
}
