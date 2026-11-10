using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GarageManagement.BUS;
using GarageManagement.DTO;
using static GarageManagement.GUI.UiHelper;

namespace GarageManagement.GUI
{
    public class FrmMain : Form
    {
        private readonly XeBUS xeBus = new XeBUS();
        private readonly PhuTungBUS ptBus = new PhuTungBUS();
        private readonly PhieuSuaChuaBUS phieuBus = new PhieuSuaChuaBUS();
        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

        // Tiếp nhận xe
        private TextBox txtTimBS, txtBienSo, txtHieuXe, txtNam, txtTenKH, txtSDT, txtDiaChi;
        private DataGridView gridXe;
        private int selMaKH;

        // Phụ tùng
        private DataGridView gridPT;
        private TextBox txtTenPT;
        private NumericUpDown numGia, numTon;
        private int selPT;

        // Lập phiếu
        private ComboBox cboXePhieu, cboPTPhieu;
        private NumericUpDown numSL, numCong;
        private TextBox txtNoiDung;
        private DataGridView gridGio;
        private Label lblTong;
        private readonly List<ChiTietSuaChuaDTO> gio = new List<ChiTietSuaChuaDTO>();

        // Phiếu & thanh toán
        private DataGridView gridPhieu, gridCT;

        // Báo cáo
        private DataGridView gridDT, gridSapHet;
        private Label lblTongQuan;

        public FrmMain()
        {
            Text = "Hệ thống Quản lý Garage & Bảo dưỡng xe ô tô (3-Tier)";
            Size = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;
            var header = new Label
            {
                Text = "  🚗 GARAGE MANAGEMENT SYSTEM", Dock = DockStyle.Top, Height = 48, BackColor = Primary,
                ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft
            };
            tabs.TabPages.Add(BuildTongQuan());
            tabs.TabPages.Add(BuildTiepNhan());
            tabs.TabPages.Add(BuildPhuTung());
            tabs.TabPages.Add(BuildLapPhieu());
            tabs.TabPages.Add(BuildPhieu());
            tabs.TabPages.Add(BuildBaoCao());
            tabs.SelectedIndexChanged += (s, e) => Reload();
            Controls.Add(tabs);
            Controls.Add(header);
            Load += (s, e) => Reload();
        }

        private void Reload()
        {
            try
            {
                switch (tabs.SelectedIndex)
                {
                    case 0: LoadTongQuan(); break;
                    case 1: gridXe.DataSource = xeBus.TraCuu(txtTimBS.Text); HeadersXe(); break;
                    case 2: LoadPT(); break;
                    case 3: LoadLapPhieu(); break;
                    case 4: LoadPhieu(); break;
                    case 5: LoadBaoCao(); break;
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        // ============ TỔNG QUAN ============
        private TabPage BuildTongQuan()
        {
            var tp = new TabPage("Tổng quan") { BackColor = Color.White };
            lblTongQuan = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13F), Padding = new Padding(30) };
            tp.Controls.Add(lblTongQuan);
            return tp;
        }

        private void LoadTongQuan()
        {
            DataTable phieu = phieuBus.GetAll();
            Func<string, int> dem = st => phieu.AsEnumerable().Count(r => r.Field<string>("TrangThai") == st);
            decimal dt = phieu.AsEnumerable().Where(r => r.Field<string>("TrangThai") == "Đã thanh toán").Sum(r => r.Field<decimal>("TongTien"));
            lblTongQuan.Text =
                "TỔNG QUAN GARAGE\n\n" +
                $"🚘  Số xe đã tiếp nhận: {xeBus.TraCuu("").Rows.Count}\n" +
                $"🔧  Số loại phụ tùng: {ptBus.GetAll().Rows.Count}   (sắp hết hàng: {phieuBus.PhuTungSapHet().Rows.Count})\n" +
                $"📋  Phiếu sửa chữa: {phieu.Rows.Count}  —  Đang sửa: {dem("Đang sửa")}, Đã hoàn thành: {dem("Đã hoàn thành")}, Đã thanh toán: {dem("Đã thanh toán")}\n" +
                $"💰  Tổng doanh thu: {dt:N0} VNĐ\n\n" +
                "Quy trình: Tiếp nhận xe → Lập phiếu sửa chữa (SqlTransaction trừ kho) → Hoàn thành → Thanh toán & xuất hóa đơn.";
        }

        // ============ TIẾP NHẬN XE ============
        private TabPage BuildTiepNhan()
        {
            var tp = new TabPage("Tiếp nhận xe & Chủ xe") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 175 };
            txtTimBS = new TextBox { Left = 130, Top = 12, Width = 200 };
            var btnTim = Btn("🔍 Tra cứu", 340, 8, null, 110);
            txtBienSo = new TextBox { Left = 130, Top = 55, Width = 200 };
            txtHieuXe = new TextBox { Left = 130, Top = 90, Width = 200 };
            txtNam = new TextBox { Left = 130, Top = 125, Width = 200 };
            txtTenKH = new TextBox { Left = 480, Top = 55, Width = 250 };
            txtSDT = new TextBox { Left = 480, Top = 90, Width = 250 };
            txtDiaChi = new TextBox { Left = 480, Top = 125, Width = 250 };
            var btnThem = Btn("Nhập xe mới", 780, 55, Color.FromArgb(22, 163, 74), 120);
            var btnSua = Btn("Sửa", 910, 55, Color.FromArgb(234, 88, 12));
            var btnXoa = Btn("Xóa", 1020, 55, Color.FromArgb(220, 38, 38));
            var btnMoi = Btn("Làm mới", 780, 100, Color.Gray, 120);
            pnl.Controls.AddRange(new Control[]
            {
                Lbl("Tra biển số:", 20, 12), txtTimBS, btnTim,
                Lbl("Biển số:", 20, 55), txtBienSo, Lbl("Hiệu xe:", 20, 90), txtHieuXe, Lbl("Năm SX:", 20, 125), txtNam,
                Lbl("Chủ xe:", 360, 55), txtTenKH, Lbl("Điện thoại:", 360, 90), txtSDT, Lbl("Địa chỉ:", 360, 125), txtDiaChi,
                btnThem, btnSua, btnXoa, btnMoi
            });
            gridXe = Grid(); gridXe.Dock = DockStyle.Fill;
            gridXe.SelectionChanged += (s, e) =>
            {
                if (gridXe.CurrentRow?.DataBoundItem is DataRowView r)
                {
                    txtBienSo.Text = r["BienSo"].ToString(); txtHieuXe.Text = r["HieuXe"].ToString(); txtNam.Text = r["NamSanXuat"].ToString();
                    txtTenKH.Text = r["TenKH"].ToString(); txtSDT.Text = r["DienThoai"].ToString(); txtDiaChi.Text = r["DiaChi"].ToString();
                    selMaKH = (int)r["MaKH"];
                }
            };
            btnTim.Click += (s, e) => Try(() =>
            {
                DataTable dt = xeBus.TraCuu(txtTimBS.Text);
                gridXe.DataSource = dt; HeadersXe();
                if (dt.Rows.Count == 0 && Confirm("Không tìm thấy xe. Nhập xe mới với biển số này?"))
                {
                    txtBienSo.Text = txtTimBS.Text.Trim().ToUpper(); txtHieuXe.Clear(); txtNam.Clear(); txtTenKH.Clear(); txtSDT.Clear(); txtDiaChi.Clear();
                    txtTenKH.Focus();
                }
            });
            Func<KhachHangDTO> kh = () => new KhachHangDTO { MaKH = selMaKH, TenKH = txtTenKH.Text.Trim(), DienThoai = txtSDT.Text.Trim(), DiaChi = txtDiaChi.Text.Trim() };
            Func<XeDTO> xe = () => new XeDTO
            {
                BienSo = txtBienSo.Text.Trim(), HieuXe = txtHieuXe.Text.Trim(),
                NamSanXuat = int.TryParse(txtNam.Text, out int n) ? n : (int?)null
            };
            btnThem.Click += (s, e) => Try(() => { xeBus.TiepNhanXeMoi(kh(), xe()); txtTimBS.Clear(); Reload(); Info("Đã tiếp nhận xe mới."); });
            btnSua.Click += (s, e) => Try(() => { if (selMaKH == 0) throw new Exception("Chọn xe cần sửa."); xeBus.Update(kh(), xe()); Reload(); Info("Đã cập nhật."); });
            btnXoa.Click += (s, e) => Try(() => { if (Confirm("Xóa xe " + txtBienSo.Text + "?")) { xeBus.Delete(txtBienSo.Text.Trim()); Reload(); } });
            btnMoi.Click += (s, e) => { txtTimBS.Clear(); selMaKH = 0; Reload(); };
            tp.Controls.Add(gridXe);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void HeadersXe() => SetHeaders(gridXe, ("BienSo", "Biển số"), ("HieuXe", "Hiệu xe"), ("NamSanXuat", "Năm SX"),
            ("MaKH", "Mã KH"), ("TenKH", "Chủ xe"), ("DienThoai", "Điện thoại"), ("DiaChi", "Địa chỉ"));

        // ============ PHỤ TÙNG ============
        private TabPage BuildPhuTung()
        {
            var tp = new TabPage("Kho phụ tùng") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            txtTenPT = new TextBox { Left = 130, Top = 15, Width = 300 };
            numGia = new NumericUpDown { Left = 130, Top = 50, Width = 200, Maximum = 1000000000, ThousandsSeparator = true, Increment = 10000 };
            numTon = new NumericUpDown { Left = 130, Top = 85, Width = 200, Maximum = 100000 };
            var btnThem = Btn("Thêm", 640, 15, Color.FromArgb(22, 163, 74));
            var btnSua = Btn("Cập nhật", 750, 15, Color.FromArgb(234, 88, 12));
            var btnXoa = Btn("Xóa", 860, 15, Color.FromArgb(220, 38, 38));
            pnl.Controls.AddRange(new Control[] { Lbl("Tên phụ tùng:", 20, 15), txtTenPT, Lbl("Đơn giá:", 20, 50), numGia, Lbl("Số lượng tồn:", 20, 85), numTon, btnThem, btnSua, btnXoa });
            gridPT = Grid(); gridPT.Dock = DockStyle.Fill;
            gridPT.SelectionChanged += (s, e) =>
            {
                if (gridPT.CurrentRow?.DataBoundItem is DataRowView r)
                {
                    selPT = (int)r["MaPhuTung"]; txtTenPT.Text = r["TenPhuTung"].ToString();
                    numGia.Value = (decimal)r["DonGia"]; numTon.Value = (int)r["SoLuongTon"];
                }
            };
            gridPT.CellFormatting += (s, e) =>
            {
                if (gridPT.Columns[e.ColumnIndex].Name == "SoLuongTon" && e.Value is int ton && ton <= 5)
                { e.CellStyle.ForeColor = Color.Red; e.CellStyle.Font = new Font(gridPT.Font, FontStyle.Bold); }
            };
            Func<PhuTungDTO> read = () => new PhuTungDTO { MaPhuTung = selPT, TenPhuTung = txtTenPT.Text.Trim(), DonGia = numGia.Value, SoLuongTon = (int)numTon.Value };
            btnThem.Click += (s, e) => Try(() => { ptBus.Insert(read()); LoadPT(); Info("Đã thêm phụ tùng."); });
            btnSua.Click += (s, e) => Try(() => { if (selPT == 0) throw new Exception("Chọn phụ tùng."); ptBus.Update(read()); LoadPT(); Info("Đã cập nhật giá / tồn kho."); });
            btnXoa.Click += (s, e) => Try(() => { if (selPT != 0 && Confirm("Xóa phụ tùng?")) { ptBus.Delete(selPT); LoadPT(); } });
            tp.Controls.Add(gridPT);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void LoadPT()
        {
            gridPT.DataSource = ptBus.GetAll();
            SetHeaders(gridPT, ("MaPhuTung", "Mã"), ("TenPhuTung", "Tên phụ tùng"), ("DonGia", "Đơn giá"), ("SoLuongTon", "Tồn kho"));
            if (gridPT.Columns.Contains("DonGia")) gridPT.Columns["DonGia"].DefaultCellStyle.Format = "N0";
        }

        // ============ LẬP PHIẾU ============
        private TabPage BuildLapPhieu()
        {
            var tp = new TabPage("Lập phiếu sửa chữa") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 170 };
            cboXePhieu = new ComboBox { Left = 130, Top = 15, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            txtNoiDung = new TextBox { Left = 130, Top = 50, Width = 300, Height = 50, Multiline = true };
            cboPTPhieu = new ComboBox { Left = 560, Top = 15, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            numSL = new NumericUpDown { Left = 560, Top = 50, Width = 100, Minimum = 1, Maximum = 1000, Value = 1 };
            numCong = new NumericUpDown { Left = 560, Top = 85, Width = 150, Maximum = 100000000, ThousandsSeparator = true, Increment = 50000 };
            var btnAdd = Btn("+ Thêm vào phiếu", 880, 12, Color.FromArgb(37, 99, 235), 160);
            var btnRemove = Btn("− Bỏ dòng", 880, 50, Color.Gray, 160);
            var btnLuu = Btn("💾 LƯU PHIẾU (Transaction)", 880, 110, Color.FromArgb(22, 163, 74), 240);
            lblTong = new Label { Left = 130, Top = 120, Width = 500, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28), Text = "Tổng chi phí: 0 VNĐ" };
            pnl.Controls.AddRange(new Control[]
            {
                Lbl("Xe (biển số):", 20, 15), cboXePhieu, Lbl("Nội dung sửa:", 20, 50), txtNoiDung,
                Lbl("Phụ tùng:", 450, 15), cboPTPhieu, Lbl("Số lượng:", 450, 50), numSL, Lbl("Tiền công:", 450, 85), numCong,
                btnAdd, btnRemove, btnLuu, lblTong
            });
            gridGio = Grid(); gridGio.Dock = DockStyle.Fill;
            btnAdd.Click += (s, e) => Try(() =>
            {
                if (!(cboPTPhieu.SelectedItem is DataRowView pt)) throw new Exception("Chọn phụ tùng.");
                int ma = (int)pt["MaPhuTung"];
                var exist = gio.FirstOrDefault(c => c.MaPhuTung == ma);
                if (exist != null) { exist.SoLuong += (int)numSL.Value; exist.TienCong += numCong.Value; }
                else gio.Add(new ChiTietSuaChuaDTO { MaPhuTung = ma, TenPhuTung = pt["TenPhuTung"].ToString(), DonGia = (decimal)pt["DonGia"], SoLuong = (int)numSL.Value, TienCong = numCong.Value });
                RefreshGio();
            });
            btnRemove.Click += (s, e) =>
            {
                if (gridGio.CurrentRow != null && gridGio.CurrentRow.Index < gio.Count) { gio.RemoveAt(gridGio.CurrentRow.Index); RefreshGio(); }
            };
            btnLuu.Click += (s, e) =>
            {
                try
                {
                    var phieu = new PhieuSuaChuaDTO { BienSo = cboXePhieu.SelectedValue?.ToString(), NoiDungSua = txtNoiDung.Text.Trim(), ChiTiet = gio.ToList() };
                    int ma = phieuBus.LapPhieu(phieu);
                    Info($"Lập phiếu #{ma} thành công!\nĐã trừ tồn kho phụ tùng tương ứng.\nTổng chi phí: {phieu.TongTien:N0} VNĐ");
                    gio.Clear(); txtNoiDung.Clear(); LoadLapPhieu();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("GIAO DỊCH ĐÃ BỊ HỦY (trans.Rollback())\n\n" + ex.Message + "\n\nKhông có dữ liệu nào bị ghi vào CSDL.",
                        "Lỗi lập phiếu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoadLapPhieu();
                }
            };
            tp.Controls.Add(gridGio);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void LoadLapPhieu()
        {
            DataTable xe = xeBus.TraCuu("");
            xe.Columns.Add("Hien", typeof(string), "BienSo + ' - ' + TenKH + ' (' + HieuXe + ')'");
            cboXePhieu.DisplayMember = "Hien"; cboXePhieu.ValueMember = "BienSo"; cboXePhieu.DataSource = xe;
            DataTable pt = ptBus.GetAll();
            pt.Columns.Add("Hien", typeof(string), "TenPhuTung + ' | ' + Convert(DonGia, 'System.String') + 'đ | tồn: ' + Convert(SoLuongTon, 'System.String')");
            cboPTPhieu.DisplayMember = "Hien"; cboPTPhieu.DataSource = pt;
            RefreshGio();
        }

        private void RefreshGio()
        {
            gridGio.DataSource = null;
            gridGio.DataSource = gio.Select(c => new { c.MaPhuTung, c.TenPhuTung, c.SoLuong, DonGia = c.DonGia.ToString("N0"), TienCong = c.TienCong.ToString("N0"), ThanhTien = c.ThanhTien.ToString("N0") }).ToList();
            SetHeaders(gridGio, ("MaPhuTung", "Mã PT"), ("TenPhuTung", "Phụ tùng"), ("SoLuong", "SL"), ("DonGia", "Đơn giá"), ("TienCong", "Tiền công"), ("ThanhTien", "Thành tiền"));
            lblTong.Text = $"Tổng chi phí: {PhieuSuaChuaBUS.TinhTongTien(new PhieuSuaChuaDTO { ChiTiet = gio }):N0} VNĐ";
        }

        // ============ PHIẾU & THANH TOÁN ============
        private TabPage BuildPhieu()
        {
            var tp = new TabPage("Phiếu sửa chữa & Thanh toán") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 50 };
            var btnHT = Btn("✔ Hoàn thành sửa", 15, 9, Color.FromArgb(234, 88, 12), 170);
            var btnTT = Btn("💵 Thanh toán & Xuất hóa đơn", 195, 9, Color.FromArgb(22, 163, 74), 250);
            var btnHD = Btn("🧾 Xem hóa đơn", 455, 9, Color.Gray, 150);
            pnl.Controls.AddRange(new Control[] { btnHT, btnTT, btnHD });
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 330 };
            gridPhieu = Grid(); gridPhieu.Dock = DockStyle.Fill;
            gridCT = Grid(); gridCT.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(gridPhieu);
            split.Panel2.Controls.Add(gridCT);
            split.Panel2.Controls.Add(new Label { Text = "Chi tiết phụ tùng & tiền công của phiếu đang chọn", Dock = DockStyle.Top, Height = 26, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Primary });
            gridPhieu.SelectionChanged += (s, e) => Try(() =>
            {
                if (gridPhieu.CurrentRow?.DataBoundItem is DataRowView r)
                {
                    gridCT.DataSource = phieuBus.GetChiTiet((int)r["MaPhieu"]);
                    SetHeaders(gridCT, ("MaPhuTung", "Mã PT"), ("TenPhuTung", "Phụ tùng"), ("SoLuong", "SL"), ("DonGia", "Đơn giá"), ("TienCong", "Tiền công"), ("ThanhTien", "Thành tiền"));
                    foreach (var c in new[] { "DonGia", "TienCong", "ThanhTien" }) if (gridCT.Columns.Contains(c)) gridCT.Columns[c].DefaultCellStyle.Format = "N0";
                }
            });
            gridPhieu.CellFormatting += (s, e) =>
            {
                if (gridPhieu.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value != null)
                    e.CellStyle.ForeColor = e.Value.ToString() == "Đã thanh toán" ? Color.Green : e.Value.ToString() == "Đang sửa" ? Color.OrangeRed : Color.RoyalBlue;
            };
            Func<DataRowView> sel = () => gridPhieu.CurrentRow?.DataBoundItem as DataRowView ?? throw new Exception("Chọn phiếu.");
            btnHT.Click += (s, e) => Try(() => { var r = sel(); phieuBus.HoanThanh((int)r["MaPhieu"], r["TrangThai"].ToString()); LoadPhieu(); });
            btnTT.Click += (s, e) => Try(() =>
            {
                var r = sel();
                string hd = phieuBus.ThanhToan((int)r["MaPhieu"], r["TrangThai"].ToString());
                LoadPhieu();
                MessageBox.Show(hd, "Hóa đơn thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
            });
            btnHD.Click += (s, e) => Try(() => MessageBox.Show(phieuBus.XuatHoaDon((int)sel()["MaPhieu"]), "Hóa đơn"));
            tp.Controls.Add(split);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void LoadPhieu()
        {
            gridPhieu.DataSource = phieuBus.GetAll();
            SetHeaders(gridPhieu, ("MaPhieu", "Số phiếu"), ("BienSo", "Biển số"), ("TenKH", "Chủ xe"), ("NgayNhan", "Ngày nhận"), ("NoiDungSua", "Nội dung"), ("TongTien", "Tổng tiền"), ("TrangThai", "Trạng thái"));
            if (gridPhieu.Columns.Contains("TongTien")) gridPhieu.Columns["TongTien"].DefaultCellStyle.Format = "N0";
        }

        // ============ BÁO CÁO ============
        private TabPage BuildBaoCao()
        {
            var tp = new TabPage("Báo cáo - Thống kê") { BackColor = Color.White };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 280 };
            gridDT = Grid(); gridDT.Dock = DockStyle.Fill;
            gridSapHet = Grid(); gridSapHet.Dock = DockStyle.Fill; gridSapHet.DefaultCellStyle.ForeColor = Color.Red;
            split.Panel1.Controls.Add(gridDT);
            split.Panel1.Controls.Add(new Label { Text = "💰 DOANH THU THEO THÁNG (phiếu đã thanh toán)", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Primary });
            split.Panel2.Controls.Add(gridSapHet);
            split.Panel2.Controls.Add(new Label { Text = "⚠ PHỤ TÙNG SẮP HẾT (tồn ≤ 5)", Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.Red });
            tp.Controls.Add(split);
            return tp;
        }

        private void LoadBaoCao()
        {
            gridDT.DataSource = phieuBus.DoanhThuTheoThang();
            SetHeaders(gridDT, ("Thang", "Tháng"), ("SoPhieu", "Số phiếu"), ("DoanhThu", "Doanh thu (VNĐ)"));
            if (gridDT.Columns.Contains("DoanhThu")) gridDT.Columns["DoanhThu"].DefaultCellStyle.Format = "N0";
            gridSapHet.DataSource = phieuBus.PhuTungSapHet();
            SetHeaders(gridSapHet, ("MaPhuTung", "Mã"), ("TenPhuTung", "Phụ tùng"), ("SoLuongTon", "Tồn kho"));
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
