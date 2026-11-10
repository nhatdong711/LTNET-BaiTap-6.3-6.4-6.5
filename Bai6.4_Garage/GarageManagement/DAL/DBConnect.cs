using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace GarageManagement.DAL
{
    /// <summary>Lớp kết nối CSDL dùng chung, mọi truy vấn đều dùng SqlParameter (chống SQL Injection).</summary>
    public static class DBConnect
    {
        public const string DatabaseName = "GarageDB";
        private static string _connStr;

        public static string ConnectionString
        {
            get
            {
                if (_connStr == null)
                    _connStr = ConfigurationManager.ConnectionStrings["DB"]?.ConnectionString
                        ?? $"Data Source=.;Initial Catalog={DatabaseName};Integrated Security=True;TrustServerCertificate=True";
                return _connStr;
            }
        }

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        /// <summary>
        /// Tự động tìm SQL Server (máy chủ trong App.config, ".", ".\SQLEXPRESS", LocalDB)
        /// và tạo CSDL từ ScriptDB.sql nếu chưa có. Giúp chạy chương trình ngay lần đầu.
        /// </summary>
        public static void EnsureDatabase()
        {
            var cfg = new SqlConnectionStringBuilder(ConnectionString);
            string[] servers = { cfg.DataSource, ".", @".\SQLEXPRESS", @"(localdb)\MSSQLLocalDB" };
            Exception last = null;
            foreach (string server in servers)
            {
                var b = new SqlConnectionStringBuilder(ConnectionString) { DataSource = server, InitialCatalog = "master", ConnectTimeout = 5 };
                try
                {
                    using (var conn = new SqlConnection(b.ConnectionString))
                    {
                        conn.Open();
                        bool exists = Convert.ToInt32(new SqlCommand($"SELECT COUNT(*) FROM sys.databases WHERE name = '{DatabaseName}'", conn).ExecuteScalar()) > 0;
                        if (!exists) RunScript(conn);
                    }
                    b.InitialCatalog = DatabaseName;
                    b.ConnectTimeout = 15;
                    _connStr = b.ConnectionString;
                    return;
                }
                catch (Exception ex) { last = ex; }
            }
            throw new Exception("Không kết nối được SQL Server. Kiểm tra chuỗi kết nối trong App.config.\n" + last?.Message);
        }

        private static void RunScript(SqlConnection conn)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScriptDB.sql");
            if (!File.Exists(path)) throw new FileNotFoundException("Không tìm thấy ScriptDB.sql", path);
            string script = File.ReadAllText(path);
            foreach (string batch in Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(batch)) continue;
                using (var cmd = new SqlCommand(batch, conn) { CommandTimeout = 120 }) cmd.ExecuteNonQuery();
            }
            conn.ChangeDatabase("master");
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] prms)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] prms)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] prms)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (prms != null) cmd.Parameters.AddRange(prms);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
