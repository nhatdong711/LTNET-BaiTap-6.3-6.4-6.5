using System;
using System.Data;
using System.Text.RegularExpressions;
using ProjectTaskManagement.DAL;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.BUS
{
    public class ThanhVienBUS
    {
        private readonly ThanhVienDAL dal = new ThanhVienDAL();

        public DataTable GetAll() => dal.GetAll();

        private void Validate(ThanhVienDTO t)
        {
            if (string.IsNullOrWhiteSpace(t.HoTen)) throw new ArgumentException("Họ tên không được để trống.");
            if (!string.IsNullOrWhiteSpace(t.Email))
            {
                if (!Regex.IsMatch(t.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new ArgumentException("Email không hợp lệ.");
                if (dal.EmailExists(t.Email, t.MaThanhVien)) throw new ArgumentException("Email đã tồn tại.");
            }
            else t.Email = null;
        }

        public bool Insert(ThanhVienDTO t) { Validate(t); return dal.Insert(t); }
        public bool Update(ThanhVienDTO t) { Validate(t); return dal.Update(t); }
        public bool Delete(int ma) => dal.Delete(ma);
    }
}
