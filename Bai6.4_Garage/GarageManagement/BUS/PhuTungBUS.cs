using System;
using System.Data;
using GarageManagement.DAL;
using GarageManagement.DTO;

namespace GarageManagement.BUS
{
    public class PhuTungBUS
    {
        private readonly PhuTungDAL dal = new PhuTungDAL();
        public DataTable GetAll() => dal.GetAll();

        private static void Validate(PhuTungDTO p)
        {
            if (string.IsNullOrWhiteSpace(p.TenPhuTung)) throw new ArgumentException("Tên phụ tùng không được để trống.");
            if (p.DonGia < 0) throw new ArgumentException("Đơn giá không được âm.");
            if (p.SoLuongTon < 0) throw new ArgumentException("Số lượng tồn không được âm.");
        }

        public bool Insert(PhuTungDTO p) { Validate(p); return dal.Insert(p); }
        public bool Update(PhuTungDTO p) { Validate(p); return dal.Update(p); }
        public bool Delete(int ma) => dal.Delete(ma);
    }
}
