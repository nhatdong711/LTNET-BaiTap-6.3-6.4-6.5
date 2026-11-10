using System.Data;
using Microsoft.Data.SqlClient;
using GarageManagement.DTO;

namespace GarageManagement.DAL
{
    public class PhuTungDAL
    {
        public DataTable GetAll() => DBConnect.ExecuteQuery("SELECT MaPhuTung, TenPhuTung, DonGia, SoLuongTon FROM PhuTung ORDER BY MaPhuTung");

        public bool Insert(PhuTungDTO p) => DBConnect.ExecuteNonQuery(
            "INSERT INTO PhuTung(TenPhuTung, DonGia, SoLuongTon) VALUES(@ten,@gia,@sl)",
            new SqlParameter("@ten", p.TenPhuTung), new SqlParameter("@gia", p.DonGia), new SqlParameter("@sl", p.SoLuongTon)) > 0;

        public bool Update(PhuTungDTO p) => DBConnect.ExecuteNonQuery(
            "UPDATE PhuTung SET TenPhuTung=@ten, DonGia=@gia, SoLuongTon=@sl WHERE MaPhuTung=@ma",
            new SqlParameter("@ten", p.TenPhuTung), new SqlParameter("@gia", p.DonGia),
            new SqlParameter("@sl", p.SoLuongTon), new SqlParameter("@ma", p.MaPhuTung)) > 0;

        public bool Delete(int ma) => DBConnect.ExecuteNonQuery("DELETE FROM PhuTung WHERE MaPhuTung=@ma", new SqlParameter("@ma", ma)) > 0;
    }
}
