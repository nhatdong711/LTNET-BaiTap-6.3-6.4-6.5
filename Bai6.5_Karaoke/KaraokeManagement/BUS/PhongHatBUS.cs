using System.Collections.Generic;
using KaraokeManagement.DAL;
using KaraokeManagement.DTO;

namespace KaraokeManagement.BUS
{
    public class PhongHatBUS
    {
        private readonly PhongHatDAL dal = new PhongHatDAL();
        public List<PhongHatDTO> GetAll() => dal.GetAll();
        public int MoPhong(int maPhong) => dal.MoPhong(maPhong);
    }
}
