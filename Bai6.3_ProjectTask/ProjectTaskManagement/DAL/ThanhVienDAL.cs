using System;
using System.Data;
using Microsoft.Data.SqlClient;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.DAL
{
    public class ThanhVienDAL
    {
        public DataTable GetAll() =>
            DBConnect.ExecuteQuery("SELECT MaThanhVien, HoTen, Email, ChuyenMon FROM ThanhVien ORDER BY MaThanhVien");

        public bool EmailExists(string email, int exceptId) =>
            Convert.ToInt32(DBConnect.ExecuteScalar("SELECT COUNT(*) FROM ThanhVien WHERE Email=@e AND MaThanhVien<>@id",
                new SqlParameter("@e", email), new SqlParameter("@id", exceptId))) > 0;

        public bool Insert(ThanhVienDTO t) => DBConnect.ExecuteNonQuery(
            "INSERT INTO ThanhVien(HoTen, Email, ChuyenMon) VALUES(@ten, @email, @cm)",
            new SqlParameter("@ten", t.HoTen),
            new SqlParameter("@email", (object)t.Email ?? DBNull.Value),
            new SqlParameter("@cm", (object)t.ChuyenMon ?? DBNull.Value)) > 0;

        public bool Update(ThanhVienDTO t) => DBConnect.ExecuteNonQuery(
            "UPDATE ThanhVien SET HoTen=@ten, Email=@email, ChuyenMon=@cm WHERE MaThanhVien=@ma",
            new SqlParameter("@ten", t.HoTen),
            new SqlParameter("@email", (object)t.Email ?? DBNull.Value),
            new SqlParameter("@cm", (object)t.ChuyenMon ?? DBNull.Value),
            new SqlParameter("@ma", t.MaThanhVien)) > 0;

        public bool Delete(int ma)
        {
            DBConnect.ExecuteNonQuery("DELETE FROM PhanCong WHERE MaThanhVien=@ma", new SqlParameter("@ma", ma));
            return DBConnect.ExecuteNonQuery("DELETE FROM ThanhVien WHERE MaThanhVien=@ma", new SqlParameter("@ma", ma)) > 0;
        }
    }
}
