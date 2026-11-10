using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace ProjectTaskManagement.DAL
{
    public class PhanCongDAL
    {
        public List<int> GetThanhVienOfTask(int maTask)
        {
            var list = new List<int>();
            using (var conn = DBConnect.GetConnection())
            using (var cmd = new SqlCommand("SELECT MaThanhVien FROM PhanCong WHERE MaTask=@ma", conn))
            {
                cmd.Parameters.AddWithValue("@ma", maTask);
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read()) list.Add(rd.GetInt32(0));
            }
            return list;
        }

        /// <summary>Gán lại danh sách thành viên cho 1 task (một task nhiều thành viên).</summary>
        public void SetThanhVien(int maTask, IEnumerable<int> dsThanhVien)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var del = new SqlCommand("DELETE FROM PhanCong WHERE MaTask=@ma", conn, tran);
                        del.Parameters.AddWithValue("@ma", maTask);
                        del.ExecuteNonQuery();
                        foreach (int tv in dsThanhVien)
                        {
                            var ins = new SqlCommand("INSERT INTO PhanCong(MaTask, MaThanhVien, NgayGiao) VALUES(@t, @v, GETDATE())", conn, tran);
                            ins.Parameters.AddWithValue("@t", maTask);
                            ins.Parameters.AddWithValue("@v", tv);
                            ins.ExecuteNonQuery();
                        }
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
    }
}
