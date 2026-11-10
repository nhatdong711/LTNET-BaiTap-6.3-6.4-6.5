using System.Data;
using Microsoft.Data.SqlClient;

namespace KaraokeManagement.DAL
{
    /// <summary>
    /// Mô hình ngắt kết nối (Disconnected Model): SqlDataAdapter đổ dữ liệu vào DataTable,
    /// người dùng thao tác trên DataTable rồi dùng adapter.Update() để đồng bộ về CSDL.
    /// </summary>
    public class DichVuDAL
    {
        private SqlDataAdapter adapter;

        public DataTable LoadDichVu()
        {
            var conn = DBConnect.GetConnection();
            adapter = new SqlDataAdapter("SELECT MaDV, TenDV, DonGia, TonKho FROM DichVu ORDER BY MaDV", conn);
            var builder = new SqlCommandBuilder(adapter);
            adapter.InsertCommand = builder.GetInsertCommand();
            adapter.UpdateCommand = builder.GetUpdateCommand();
            adapter.DeleteCommand = builder.GetDeleteCommand();
            var dt = new DataTable("DichVu");
            adapter.Fill(dt);
            dt.Columns["MaDV"].AutoIncrement = true;
            dt.Columns["MaDV"].AutoIncrementSeed = -1;
            dt.Columns["MaDV"].AutoIncrementStep = -1;
            return dt;
        }

        public int SaveDichVu(DataTable dt) => adapter.Update(dt);
    }
}
