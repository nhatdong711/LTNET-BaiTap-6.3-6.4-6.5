using System;
using System.Data;
using KaraokeManagement.DAL;

namespace KaraokeManagement.BUS
{
    public class DichVuBUS
    {
        private readonly DichVuDAL dal = new DichVuDAL();
        public DataTable LoadDichVu() => dal.LoadDichVu();

        public int Save(DataTable dt)
        {
            foreach (DataRow r in dt.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (string.IsNullOrWhiteSpace(r["TenDV"]?.ToString())) throw new ArgumentException("Tên dịch vụ không được trống.");
                if (r["DonGia"] == DBNull.Value || Convert.ToDecimal(r["DonGia"]) < 0) throw new ArgumentException("Đơn giá không hợp lệ.");
                if (r["TonKho"] == DBNull.Value) r["TonKho"] = 0;
                if (Convert.ToInt32(r["TonKho"]) < 0) throw new ArgumentException("Tồn kho không được âm.");
            }
            return dal.SaveDichVu(dt);
        }
    }
}
