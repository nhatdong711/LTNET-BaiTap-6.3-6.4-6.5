using System;
using System.Data;
using ProjectTaskManagement.DAL;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.BUS
{
    public class DuAnBUS
    {
        private readonly DuAnDAL dal = new DuAnDAL();
        public static readonly string[] TrangThaiList = { "Đang chạy", "Hoàn thành", "Tạm dừng" };

        public DataTable GetAll() => dal.GetAll();

        private void Validate(DuAnDTO d)
        {
            if (string.IsNullOrWhiteSpace(d.TenDuAn)) throw new ArgumentException("Tên dự án không được để trống.");
            if (d.TenDuAn.Length > 150) throw new ArgumentException("Tên dự án tối đa 150 ký tự.");
            if (d.NgayBatDau.HasValue && d.NgayKetThuc.HasValue && d.NgayKetThuc < d.NgayBatDau)
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
        }

        public bool Insert(DuAnDTO d) { Validate(d); return dal.Insert(d); }
        public bool Update(DuAnDTO d) { Validate(d); return dal.Update(d); }
        public bool Delete(int ma) => dal.Delete(ma);
    }
}
