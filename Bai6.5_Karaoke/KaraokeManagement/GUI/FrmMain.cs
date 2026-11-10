using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using KaraokeManagement.BUS;
using KaraokeManagement.DTO;
using static KaraokeManagement.GUI.UiHelper;

namespace KaraokeManagement.GUI
{
    public class FrmMain : Form
    {
        private readonly PhongHatBUS phongBus = new PhongHatBUS();
        private readonly DichVuBUS dvBus = new DichVuBUS();
        private readonly HoaDonBUS hdBus = new HoaDonBUS();
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };
        private readonly Timer timer = new Timer { Interval = 1000 };

        private static readonly Color Green = Color.FromArgb(22, 163, 74);
        private static readonly Color Red = Color.FromArgb(220, 38, 38);

        // Sơ đồ phòng
        private FlowLayoutPanel flpPhong;
        private Label lblPhong, lblThoiGian, lblTien, lblThongKe;
        private Button btnMo, btnThanhToan, btnThemDV, btnXoaDV;
        private DataGridView gridMenu, gridOrder;
        private NumericUpDown numSL;
        private PhongHatDTO phongChon;
        private HoaDonDTO hdHienTai;
        private DataTable menuTable;

        // Quản lý dịch vụ (disconnected)
        private DataGridView gridDV;
        private DataTable dvTable;

        // Báo cáo
        private DataGridView gridDoanhThu, gridLichSu;

        public FrmMain()
        {
            Text = "Hệ thống Quản lý Đặt phòng & Dịch vụ Karaoke (3-Tier)";
            Size = new Size(1300, 780);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            var header = new Label
            {
                Text = "  🎤 KARAOKE MANAGEMENT SYSTEM", Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(88, 28, 135),
                ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft
            };
            tabs.TabPages.Add(BuildSoDo());
            tabs.TabPages.Add(BuildDichVu());
            tabs.TabPages.Add(BuildBaoCao());
            tabs.SelectedIndexChanged += (s, e) => Reload();
            Controls.Add(tabs);
            Controls.Add(header);
            Load += (s, e) => Reload();
            timer.Tick += (s, e) => CapNhatTamTinh();
            timer.Start();
        }

        private void Reload()
        {
            try
            {
                switch (tabs.SelectedIndex)
                {
                    case 0: LoadSoDo(); break;
                    case 1: LoadDichVu(); break;
                    case 2: LoadBaoCao(); break;
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        // ================= SƠ ĐỒ PHÒNG =================
        private TabPage BuildSoDo()
        {
            var tp = new TabPage("Sơ đồ phòng hát") { BackColor = Color.White };
            flpPhong = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(15), BackColor = Color.FromArgb(248, 250, 252) };
            lblThongKe = new Label { Dock = DockStyle.Top, Height = 34, Font = new Font("Segoe UI", 10.5F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0) };

            var right = new Panel { Dock = DockStyle.Right, Width = 520, Padding = new Padding(10), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            lblPhong = new Label { Left = 10, Top = 10, Width = 490, Height = 30, Font = new Font("Segoe UI", 14F, FontStyle.Bold), ForeColor = Color.FromArgb(88, 28, 135), Text = "Chọn một phòng bên trái" };
            lblThoiGian = new Label { Left = 10, Top = 42, Width = 490, Height = 22 };
            lblTien = new Label { Left = 10, Top = 64, Width = 490, Height = 24, Font = new Font("Segoe UI", 11F, FontStyle.Bold), ForeColor = Red };
            btnMo = Btn("▶ MỞ PHÒNG", 10, 92, Green, 150);
            btnThanhToan = Btn("💵 THANH TOÁN", 170, 92, Red, 160);

            gridMenu = Grid(); gridMenu.SetBounds(10, 135, 300, 230);
            numSL = new NumericUpDown { Left = 320, Top = 135, Width = 80, Minimum = 1, Maximum = 100, Value = 1 };
            btnThemDV = Btn("+ Gọi món", 320, 170, Color.FromArgb(37, 99, 235), 170);
            btnXoaDV = Btn("− Bỏ món", 320, 210, Color.Gray, 170);
            var lblOrder = new Label { Left = 10, Top = 372, Width = 400, Text = "Dịch vụ đã gọi của phòng:", Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            gridOrder = Grid(); gridOrder.SetBounds(10, 395, 490, 230);
            right.Controls.AddRange(new Control[] { lblPhong, lblThoiGian, lblTien, btnMo, btnThanhToan, gridMenu, numSL, btnThemDV, btnXoaDV, lblOrder, gridOrder });
            numSL.Top = 135; numSL.Left = 410;
            right.Controls.Add(new Label { Left = 320, Top = 138, Width = 85, Text = "Số lượng:" });

            btnMo.Click += (s, e) => Try(() =>
            {
                if (phongChon == null) throw new Exception("Chọn phòng.");
                int maHD = phongBus.MoPhong(phongChon.MaPhong);
                Info($"Đã mở {phongChon.TenPhong}. Tạo hóa đơn #{maHD}, giờ vào {DateTime.Now:HH:mm}.");
                int ma = phongChon.MaPhong;
                LoadSoDo(); ChonPhong(phongBus.GetAll().First(p => p.MaPhong == ma));
            });
            btnThemDV.Click += (s, e) => Try(() =>
            {
                if (hdHienTai == null) throw new Exception("Phòng chưa mở.");
                if (!(gridMenu.CurrentRow?.DataBoundItem is DataRowView dv)) throw new Exception("Chọn món trong danh sách.");
                hdBus.ThemDichVu(hdHienTai.MaHD, dv.Row, (int)numSL.Value);
                LoadOrder();
            });
            btnXoaDV.Click += (s, e) => Try(() =>
            {
                if (hdHienTai != null && gridOrder.CurrentRow?.DataBoundItem is DataRowView r)
                { hdBus.XoaDichVu(hdHienTai.MaHD, (int)r["MaDV"]); LoadOrder(); }
            });
            btnThanhToan.Click += (s, e) =>
            {
                if (hdHienTai == null) { Info("Phòng chưa mở."); return; }
                try
                {
                    var preview = hdBus.TinhTien(hdHienTai, DateTime.Now);
                    if (!Confirm(hdBus.InHoaDon(preview) + "\nXác nhận thanh toán?")) return;
                    string bill = hdBus.ThanhToan(hdHienTai);
                    MessageBox.Show(bill + "\n✔ Đã thanh toán (SqlTransaction Commit).", "Hóa đơn", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    int ma = phongChon.MaPhong;
                    LoadSoDo(); ChonPhong(phongBus.GetAll().First(p => p.MaPhong == ma));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("THANH TOÁN THẤT BẠI - trans.Rollback()\n\n" + ex.Message + "\n\nDữ liệu hóa đơn, kho và phòng được giữ nguyên.",
                        "Lỗi giao dịch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            tp.Controls.Add(flpPhong);
            tp.Controls.Add(lblThongKe);
            tp.Controls.Add(right);
            return tp;
        }

        private void LoadSoDo()
        {
            var list = phongBus.GetAll();
            flpPhong.SuspendLayout();
            flpPhong.Controls.Clear();
            foreach (var p in list)
            {
                bool trong = p.TrangThai == "Trống";
                var b = new Button
                {
                    Width = 165, Height = 120, Margin = new Padding(8), FlatStyle = FlatStyle.Flat,
                    BackColor = trong ? Green : Red, ForeColor = Color.White, Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Text = $"{(p.LoaiPhong == "VIP" ? "👑 " : "🎤 ")}{p.TenPhong}\n{p.LoaiPhong}\n{p.GiaTheoGio:N0}đ/giờ\n[{p.TrangThai}]",
                    Tag = p
                };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (s, e) => Try(() => ChonPhong((PhongHatDTO)((Button)s).Tag));
                flpPhong.Controls.Add(b);
            }
            flpPhong.ResumeLayout();
            int dangHat = list.Count(p => p.TrangThai != "Trống");
            lblThongKe.Text = $"Tổng {list.Count} phòng   |   🟩 Trống: {list.Count - dangHat}   |   🟥 Đang hát: {dangHat}";

            menuTable = dvBus.LoadDichVu();
            gridMenu.DataSource = menuTable;
            SetHeaders(gridMenu, ("MaDV", "Mã"), ("TenDV", "Món"), ("DonGia", "Giá"), ("TonKho", "Tồn"));
            if (gridMenu.Columns.Contains("MaDV")) gridMenu.Columns["MaDV"].Visible = false;
            if (gridMenu.Columns.Contains("DonGia")) gridMenu.Columns["DonGia"].DefaultCellStyle.Format = "N0";
            if (phongChon == null) SetButtons();
        }

        private void ChonPhong(PhongHatDTO p)
        {
            phongChon = p;
            hdHienTai = p.TrangThai == "Trống" ? null : hdBus.GetHoaDonDangMo(p.MaPhong);
            lblPhong.Text = $"{p.TenPhong} ({p.LoaiPhong}) - {p.TrangThai}";
            foreach (Button b in flpPhong.Controls)
                b.FlatAppearance.BorderSize = ((PhongHatDTO)b.Tag).MaPhong == p.MaPhong ? 4 : 0;
            foreach (Button b in flpPhong.Controls)
                b.FlatAppearance.BorderColor = Color.Gold;
            SetButtons();
            LoadOrder();
        }

        private void SetButtons()
        {
            bool mo = hdHienTai != null;
            btnMo.Enabled = phongChon != null && !mo;
            btnThanhToan.Enabled = btnThemDV.Enabled = btnXoaDV.Enabled = mo;
        }

        private void LoadOrder()
        {
            if (hdHienTai == null)
            {
                gridOrder.DataSource = null; lblThoiGian.Text = phongChon == null ? "" : "Phòng đang trống — bấm MỞ PHÒNG để nhận khách."; lblTien.Text = "";
                return;
            }
            gridOrder.DataSource = hdBus.GetChiTietDichVu(hdHienTai.MaHD);
            SetHeaders(gridOrder, ("MaDV", "Mã"), ("TenDV", "Món"), ("SoLuong", "SL"), ("DonGia", "Đơn giá"), ("ThanhTien", "Thành tiền"));
            foreach (var c in new[] { "DonGia", "ThanhTien" }) if (gridOrder.Columns.Contains(c)) gridOrder.Columns[c].DefaultCellStyle.Format = "N0";
            CapNhatTamTinh();
        }

        private void CapNhatTamTinh()
        {
            if (hdHienTai == null || tabs.SelectedIndex != 0) return;
            TimeSpan t = DateTime.Now - hdHienTai.GioVao;
            decimal dv = gridOrder.DataSource is DataTable dt ? dt.AsEnumerable().Sum(r => r.Field<decimal>("ThanhTien")) : 0;
            decimal phong = Math.Round((decimal)t.TotalHours * hdHienTai.GiaTheoGio, 0);
            lblThoiGian.Text = $"HĐ #{hdHienTai.MaHD} | Giờ vào: {hdHienTai.GioVao:HH:mm}  |  Đã hát: {(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}";
            lblTien.Text = $"Tạm tính: phòng {phong:N0} + dịch vụ {dv:N0} = {phong + dv:N0} VNĐ";
        }

        // ================= DỊCH VỤ (Disconnected) =================
        private TabPage BuildDichVu()
        {
            var tp = new TabPage("Quản lý dịch vụ / đồ uống") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 54 };
            var btnLuu = Btn("💾 Lưu thay đổi (adapter.Update)", 15, 10, Green, 280);
            var btnHuy = Btn("↺ Hủy thay đổi", 305, 10, Color.Gray, 150);
            pnl.Controls.Add(btnLuu); pnl.Controls.Add(btnHuy);
            pnl.Controls.Add(new Label { Left = 470, Top = 16, Width = 700, ForeColor = Color.DimGray, Text = "Sửa trực tiếp trên lưới, thêm dòng mới ở cuối, chọn dòng + Delete để xóa. Dữ liệu chỉ ghi về CSDL khi bấm Lưu." });
            gridDV = Grid(); gridDV.Dock = DockStyle.Fill;
            gridDV.ReadOnly = false; gridDV.AllowUserToAddRows = true; gridDV.AllowUserToDeleteRows = true;
            gridDV.DataError += (s, e) => { Info("Giá trị không hợp lệ: " + e.Exception.Message); e.Cancel = true; };
            btnLuu.Click += (s, e) => Try(() => { int n = dvBus.Save(dvTable); Info($"Đã đồng bộ {n} dòng thay đổi về CSDL."); LoadDichVu(); });
            btnHuy.Click += (s, e) => { dvTable.RejectChanges(); };
            tp.Controls.Add(gridDV);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void LoadDichVu()
        {
            dvTable = dvBus.LoadDichVu();
            gridDV.DataSource = dvTable;
            gridDV.Columns["MaDV"].ReadOnly = true;
            SetHeaders(gridDV, ("MaDV", "Mã DV"), ("TenDV", "Tên dịch vụ"), ("DonGia", "Đơn giá"), ("TonKho", "Tồn kho"));
            gridDV.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        }

        // ================= BÁO CÁO =================
        private TabPage BuildBaoCao()
        {
            var tp = new TabPage("Báo cáo doanh thu") { BackColor = Color.White };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 230 };
            gridDoanhThu = Grid(); gridDoanhThu.Dock = DockStyle.Fill;
            gridLichSu = Grid(); gridLichSu.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(gridDoanhThu);
            split.Panel1.Controls.Add(new Label { Text = "💰 DOANH THU THEO NGÀY (GROUP BY)", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Primary });
            split.Panel2.Controls.Add(gridLichSu);
            split.Panel2.Controls.Add(new Label { Text = "🧾 LỊCH SỬ HÓA ĐƠN", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Primary });
            tp.Controls.Add(split);
            return tp;
        }

        private void LoadBaoCao()
        {
            gridDoanhThu.DataSource = hdBus.DoanhThuTheoNgay();
            SetHeaders(gridDoanhThu, ("Ngay", "Ngày"), ("SoHoaDon", "Số HĐ"), ("TienPhong", "Tiền phòng"), ("TienDichVu", "Tiền dịch vụ"), ("TongDoanhThu", "Tổng doanh thu"));
            gridLichSu.DataSource = hdBus.LichSuHoaDon();
            SetHeaders(gridLichSu, ("MaHD", "Mã HĐ"), ("TenPhong", "Phòng"), ("GioVao", "Giờ vào"), ("GioRa", "Giờ ra"), ("TongTienPhong", "Tiền phòng"), ("TongTienDichVu", "Tiền DV"), ("TongCong", "Tổng cộng"), ("TrangThai", "Trạng thái"));
            foreach (var g in new[] { gridDoanhThu, gridLichSu })
                foreach (DataGridViewColumn c in g.Columns)
                    if (c.ValueType == typeof(decimal)) c.DefaultCellStyle.Format = "N0";
        }

        private static void SetHeaders(DataGridView g, params (string col, string header)[] map)
        {
            foreach (var m in map) if (g.Columns.Contains(m.col)) g.Columns[m.col].HeaderText = m.header;
        }

        private static void Try(Action a)
        {
            try { a(); } catch (Exception ex) { Error(ex); }
        }
    }
}
