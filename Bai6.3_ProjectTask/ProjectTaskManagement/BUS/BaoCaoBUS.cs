using System.Data;
using ProjectTaskManagement.DAL;

namespace ProjectTaskManagement.BUS
{
    public class BaoCaoBUS
    {
        private readonly BaoCaoDAL dal = new BaoCaoDAL();
        public DataTable GetCongViecTreHan() => dal.GetCongViecTreHan();
        public DataTable GetTienDoDuAn() => dal.GetTienDoDuAn();
        public decimal GetPhanTramHoanThanh(int maDuAn) => dal.GetPhanTramHoanThanh(maDuAn);
    }
}
