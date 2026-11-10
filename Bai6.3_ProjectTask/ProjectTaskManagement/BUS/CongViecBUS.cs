using System;
using System.Collections.Generic;
using System.Data;
using ProjectTaskManagement.DAL;
using ProjectTaskManagement.DTO;

namespace ProjectTaskManagement.BUS
{
    public class CongViecBUS
    {
        private readonly CongViecDAL dal = new CongViecDAL();
        private readonly PhanCongDAL pcDal = new PhanCongDAL();
        public static readonly string[] TrangThaiList = { "To Do", "In Progress", "Review", "Done" };
        public static readonly string[] DoUuTienList = { "Cao", "Trung bình", "Thấp" };

        public DataTable Filter(int maDuAn, string trangThai) => dal.Filter(maDuAn, trangThai);

        private void Validate(CongViecDTO c)
        {
            if (c.MaDuAn <= 0) throw new ArgumentException("Vui lòng chọn dự án.");
            if (string.IsNullOrWhiteSpace(c.TenTask)) throw new ArgumentException("Tên công việc không được để trống.");
            if (Array.IndexOf(TrangThaiList, c.TrangThai) < 0) throw new ArgumentException("Trạng thái không hợp lệ.");
        }

        public int Insert(CongViecDTO c, IEnumerable<int> thanhVien)
        {
            Validate(c);
            int id = dal.Insert(c);
            pcDal.SetThanhVien(id, thanhVien);
            return id;
        }

        public bool Update(CongViecDTO c, IEnumerable<int> thanhVien)
        {
            Validate(c);
            bool ok = dal.Update(c);
            pcDal.SetThanhVien(c.MaTask, thanhVien);
            return ok;
        }

        public bool Delete(int maTask) => dal.Delete(maTask);
        public List<int> GetThanhVien(int maTask) => pcDal.GetThanhVienOfTask(maTask);

        /// <summary>Chuyển công việc sang trạng thái kế tiếp: To Do -> In Progress -> Review -> Done.</summary>
        public string ChuyenTrangThaiTiepTheo(int maTask, string hienTai)
        {
            int i = Array.IndexOf(TrangThaiList, hienTai);
            if (i < 0 || i == TrangThaiList.Length - 1) throw new InvalidOperationException("Công việc đã ở trạng thái cuối (Done).");
            string next = TrangThaiList[i + 1];
            dal.UpdateTrangThai(maTask, next);
            return next;
        }

        public bool DoiTrangThai(int maTask, string trangThai)
        {
            if (Array.IndexOf(TrangThaiList, trangThai) < 0) throw new ArgumentException("Trạng thái không hợp lệ.");
            return dal.UpdateTrangThai(maTask, trangThai);
        }
    }
}
