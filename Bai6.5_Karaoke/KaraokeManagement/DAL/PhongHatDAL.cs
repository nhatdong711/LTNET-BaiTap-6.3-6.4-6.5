using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using KaraokeManagement.DTO;

namespace KaraokeManagement.DAL
{
    public class PhongHatDAL
    {
        public List<PhongHatDTO> GetAll()
        {
            var list = new List<PhongHatDTO>();
            using (var conn = DBConnect.GetConnection())
            using (var cmd = new SqlCommand("SELECT MaPhong, TenPhong, LoaiPhong, GiaTheoGio, TrangThai FROM PhongHat ORDER BY MaPhong", conn))
            {
                conn.Open();
                using (var rd = cmd.ExecuteReader())
                    while (rd.Read())
                        list.Add(new PhongHatDTO
                        {
                            MaPhong = rd.GetInt32(0), TenPhong = rd.GetString(1), LoaiPhong = rd.IsDBNull(2) ? "" : rd.GetString(2),
                            GiaTheoGio = rd.GetDecimal(3), TrangThai = rd.GetString(4)
                        });
            }
            return list;
        }

        /// <summary>Mở phòng: tạo HoaDon mới (GioVao = GETDATE()) và đổi trạng thái phòng sang "Đang hát".</summary>
        public int MoPhong(int maPhong)
        {
            using (var conn = DBConnect.GetConnection())
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var upd = new SqlCommand("UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong = @p AND TrangThai = N'Trống'", conn, tran);
                        upd.Parameters.AddWithValue("@p", maPhong);
                        if (upd.ExecuteNonQuery() == 0) throw new InvalidOperationException("Phòng không ở trạng thái Trống.");
                        var ins = new SqlCommand("INSERT INTO HoaDon(MaPhong, GioVao, TrangThai) VALUES(@p, GETDATE(), 0); SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tran);
                        ins.Parameters.AddWithValue("@p", maPhong);
                        int maHD = Convert.ToInt32(ins.ExecuteScalar());
                        tran.Commit();
                        return maHD;
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
