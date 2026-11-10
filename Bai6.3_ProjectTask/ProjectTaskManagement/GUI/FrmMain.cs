using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ProjectTaskManagement.BUS;
using ProjectTaskManagement.DTO;
using static ProjectTaskManagement.GUI.UiHelper;

namespace ProjectTaskManagement.GUI
{
    public class FrmMain : Form
    {
        private readonly DuAnBUS duAnBus = new DuAnBUS();
        private readonly ThanhVienBUS tvBus = new ThanhVienBUS();
        private readonly CongViecBUS cvBus = new CongViecBUS();
        private readonly BaoCaoBUS bcBus = new BaoCaoBUS();

        private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10F) };

        // Tab Dự án
        private DataGridView gridDuAn;
        private TextBox txtTenDuAn;
        private DateTimePicker dtpBatDau, dtpKetThuc;
        private ComboBox cboTTDuAn;
        private int selDuAn;

        // Tab Thành viên
        private DataGridView gridTV;
        private TextBox txtHoTen, txtEmail, txtChuyenMon;
        private int selTV;

        // Tab Công việc
        private DataGridView gridCV;
        private ComboBox cboLocDuAn, cboLocTT, cboCVDuAn, cboUuTien, cboCVTT;
        private TextBox txtTenTask, txtMoTa;
        private DateTimePicker dtpHan;
        private CheckedListBox clbThanhVien;
        private FlowLayoutPanel kanban;
        private RadioButton rdoList, rdoKanban;
        private int selTask;

        // Tab Báo cáo
        private DataGridView gridTre, gridTienDo;
        private Label lblTongQuan;

        public FrmMain()
        {
            Text = "Hệ thống Quản lý Công việc & Tiến độ Dự án (3-Tier)";
            Size = new Size(1280, 760);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5F);
            BackColor = Color.White;

            var header = new Label
            {
                Text = "  PROJECT & TASK MANAGEMENT", Dock = DockStyle.Top, Height = 48,
                BackColor = Primary, ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            tabs.TabPages.Add(BuildTabTongQuan());
            tabs.TabPages.Add(BuildTabDuAn());
            tabs.TabPages.Add(BuildTabThanhVien());
            tabs.TabPages.Add(BuildTabCongViec());
            tabs.TabPages.Add(BuildTabBaoCao());
            tabs.SelectedIndexChanged += (s, e) => ReloadCurrentTab();

            Controls.Add(tabs);
            Controls.Add(header);
            Load += (s, e) => ReloadCurrentTab();
        }

        private void ReloadCurrentTab()
        {
            try
            {
                switch (tabs.SelectedIndex)
                {
                    case 0: LoadTongQuan(); break;
                    case 1: LoadDuAn(); break;
                    case 2: LoadThanhVien(); break;
                    case 3: LoadComboCongViec(); LoadCongViec(); break;
                    case 4: LoadBaoCao(); break;
                }
            }
            catch (Exception ex) { Error(ex); }
        }

        // ================= TỔNG QUAN =================
        private TabPage BuildTabTongQuan()
        {
            var tp = new TabPage("Tổng quan") { BackColor = Color.White };
            lblTongQuan = new Label { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13F), Padding = new Padding(30) };
            tp.Controls.Add(lblTongQuan);
            return tp;
        }

        private void LoadTongQuan()
        {
            DataTable da = duAnBus.GetAll();
            DataTable tv = tvBus.GetAll();
            DataTable cv = cvBus.Filter(0, null);
            DataTable tre = bcBus.GetCongViecTreHan();
            Func<string, string> ByStatus = st => cv.AsEnumerable().Count(r => r.Field<string>("TrangThai") == st).ToString();
            lblTongQuan.Text =
                "TỔNG QUAN HỆ THỐNG\n\n" +
                $"📁  Số dự án: {da.Rows.Count}\n" +
                $"👥  Số thành viên: {tv.Rows.Count}\n" +
                $"📝  Tổng công việc: {cv.Rows.Count}\n" +
                $"      • To Do: {ByStatus("To Do")}    • In Progress: {ByStatus("In Progress")}    • Review: {ByStatus("Review")}    • Done: {ByStatus("Done")}\n" +
                $"⚠️  Công việc trễ hạn: {tre.Rows.Count}\n\n" +
                "Chọn các tab phía trên để quản lý Dự án, Thành viên, Công việc (List/Kanban) và xem Báo cáo.";
        }

        // ================= DỰ ÁN =================
        private TabPage BuildTabDuAn()
        {
            var tp = new TabPage("Dự án") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            txtTenDuAn = new TextBox { Left = 130, Top = 15, Width = 350 };
            dtpBatDau = new DateTimePicker { Left = 130, Top = 50, Width = 160, Format = DateTimePickerFormat.Short };
            dtpKetThuc = new DateTimePicker { Left = 420, Top = 50, Width = 160, Format = DateTimePickerFormat.Short };
            cboTTDuAn = new ComboBox { Left = 130, Top = 85, Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
            cboTTDuAn.Items.AddRange(DuAnBUS.TrangThaiList);
            cboTTDuAn.SelectedIndex = 0;

            var btnThem = Btn("Thêm", 640, 15, Color.FromArgb(22, 163, 74));
            var btnSua = Btn("Sửa", 750, 15, Color.FromArgb(234, 88, 12));
            var btnXoa = Btn("Xóa", 860, 15, Color.FromArgb(220, 38, 38));
            var btnMoi = Btn("Làm mới", 640, 55, Color.Gray);

            pnl.Controls.AddRange(new Control[]
            {
                Lbl("Tên dự án:", 20, 15), txtTenDuAn, Lbl("Ngày bắt đầu:", 20, 50), dtpBatDau,
                Lbl("Ngày kết thúc:", 310, 50), dtpKetThuc, Lbl("Trạng thái:", 20, 85), cboTTDuAn,
                btnThem, btnSua, btnXoa, btnMoi
            });

            gridDuAn = Grid();
            gridDuAn.Dock = DockStyle.Fill;
            gridDuAn.SelectionChanged += (s, e) =>
            {
                if (gridDuAn.CurrentRow?.DataBoundItem is DataRowView r)
                {
                    selDuAn = (int)r["MaDuAn"];
                    txtTenDuAn.Text = r["TenDuAn"].ToString();
                    if (r["NgayBatDau"] != DBNull.Value) dtpBatDau.Value = (DateTime)r["NgayBatDau"];
                    if (r["NgayKetThuc"] != DBNull.Value) dtpKetThuc.Value = (DateTime)r["NgayKetThuc"];
                    cboTTDuAn.SelectedItem = r["TrangThai"].ToString();
                }
            };

            btnThem.Click += (s, e) => Try(() => { duAnBus.Insert(ReadDuAn()); LoadDuAn(); Info("Đã thêm dự án."); });
            btnSua.Click += (s, e) => Try(() =>
            {
                if (selDuAn == 0) throw new Exception("Chọn dự án cần sửa.");
                var d = ReadDuAn(); d.MaDuAn = selDuAn; duAnBus.Update(d); LoadDuAn(); Info("Đã cập nhật dự án.");
            });
            btnXoa.Click += (s, e) => Try(() =>
            {
                if (selDuAn == 0) throw new Exception("Chọn dự án cần xóa.");
                if (Confirm("Xóa dự án sẽ xóa toàn bộ công việc thuộc dự án. Tiếp tục?")) { duAnBus.Delete(selDuAn); LoadDuAn(); }
            });
            btnMoi.Click += (s, e) => { txtTenDuAn.Clear(); selDuAn = 0; LoadDuAn(); };

            tp.Controls.Add(gridDuAn);
            tp.Controls.Add(pnl);
            return tp;
        }

        private DuAnDTO ReadDuAn() => new DuAnDTO
        {
            TenDuAn = txtTenDuAn.Text.Trim(),
            NgayBatDau = dtpBatDau.Value.Date,
            NgayKetThuc = dtpKetThuc.Value.Date,
            TrangThai = cboTTDuAn.SelectedItem?.ToString()
        };

        private void LoadDuAn()
        {
            gridDuAn.DataSource = duAnBus.GetAll();
            SetHeaders(gridDuAn, ("MaDuAn", "Mã"), ("TenDuAn", "Tên dự án"), ("NgayBatDau", "Bắt đầu"), ("NgayKetThuc", "Kết thúc"), ("TrangThai", "Trạng thái"));
        }

        // ================= THÀNH VIÊN =================
        private TabPage BuildTabThanhVien()
        {
            var tp = new TabPage("Thành viên") { BackColor = Color.White };
            var pnl = new Panel { Dock = DockStyle.Top, Height = 130 };
            txtHoTen = new TextBox { Left = 130, Top = 15, Width = 300 };
            txtEmail = new TextBox { Left = 130, Top = 50, Width = 300 };
            txtChuyenMon = new TextBox { Left = 130, Top = 85, Width = 300 };
            var btnThem = Btn("Thêm", 640, 15, Color.FromArgb(22, 163, 74));
            var btnSua = Btn("Sửa", 750, 15, Color.FromArgb(234, 88, 12));
            var btnXoa = Btn("Xóa", 860, 15, Color.FromArgb(220, 38, 38));
            pnl.Controls.AddRange(new Control[]
            {
                Lbl("Họ tên:", 20, 15), txtHoTen, Lbl("Email:", 20, 50), txtEmail, Lbl("Chuyên môn:", 20, 85), txtChuyenMon,
                btnThem, btnSua, btnXoa
            });
            gridTV = Grid();
            gridTV.Dock = DockStyle.Fill;
            gridTV.SelectionChanged += (s, e) =>
            {
                if (gridTV.CurrentRow?.DataBoundItem is DataRowView r)
                {
                    selTV = (int)r["MaThanhVien"];
                    txtHoTen.Text = r["HoTen"].ToString();
                    txtEmail.Text = r["Email"].ToString();
                    txtChuyenMon.Text = r["ChuyenMon"].ToString();
                }
            };
            Func<ThanhVienDTO> Read = () => new ThanhVienDTO { MaThanhVien = selTV, HoTen = txtHoTen.Text.Trim(), Email = txtEmail.Text.Trim(), ChuyenMon = txtChuyenMon.Text.Trim() };
            btnThem.Click += (s, e) => Try(() => { var t = Read(); t.MaThanhVien = 0; tvBus.Insert(t); LoadThanhVien(); Info("Đã thêm thành viên."); });
            btnSua.Click += (s, e) => Try(() => { if (selTV == 0) throw new Exception("Chọn thành viên."); tvBus.Update(Read()); LoadThanhVien(); Info("Đã cập nhật."); });
            btnXoa.Click += (s, e) => Try(() =>
            {
                if (selTV == 0) throw new Exception("Chọn thành viên.");
                if (Confirm("Xóa thành viên này?")) { tvBus.Delete(selTV); LoadThanhVien(); }
            });
            tp.Controls.Add(gridTV);
            tp.Controls.Add(pnl);
            return tp;
        }

        private void LoadThanhVien()
        {
            gridTV.DataSource = tvBus.GetAll();
            SetHeaders(gridTV, ("MaThanhVien", "Mã"), ("HoTen", "Họ tên"), ("Email", "Email"), ("ChuyenMon", "Chuyên môn"));
        }

        // ================= CÔNG VIỆC =================
        private TabPage BuildTabCongViec()
        {
            var tp = new TabPage("Công việc (List / Kanban)") { BackColor = Color.White };

            var pnlLoc = new Panel { Dock = DockStyle.Top, Height = 48, BackColor = Color.FromArgb(241, 245, 249) };
            cboLocDuAn = new ComboBox { Left = 80, Top = 12, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocTT = new ComboBox { Left = 420, Top = 12, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            cboLocTT.Items.Add("(Tất cả)");
            cboLocTT.Items.AddRange(CongViecBUS.TrangThaiList);
            cboLocTT.SelectedIndex = 0;
            rdoList = new RadioButton { Text = "List view", Left = 620, Top = 13, Checked = true, Width = 90 };
            rdoKanban = new RadioButton { Text = "Kanban view", Left = 715, Top = 13, Width = 110 };
            var btnNext = Btn("Chuyển trạng thái ➜", 860, 8, Color.FromArgb(124, 58, 237), 170);
            pnlLoc.Controls.AddRange(new Control[] { Lbl("Dự án:", 15, 12, 60), cboLocDuAn, Lbl("Trạng thái:", 335, 12, 80), cboLocTT, rdoList, rdoKanban, btnNext });

            var pnlForm = new Panel { Dock = DockStyle.Right, Width = 380, Padding = new Padding(10) };
            cboCVDuAn = new ComboBox { Left = 120, Top = 10, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            txtTenTask = new TextBox { Left = 120, Top = 45, Width = 240 };
            txtMoTa = new TextBox { Left = 120, Top = 80, Width = 240, Height = 60, Multiline = true };
            dtpHan = new DateTimePicker { Left = 120, Top = 150, Width = 240, Format = DateTimePickerFormat.Short };
            cboUuTien = new ComboBox { Left = 120, Top = 185, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cboUuTien.Items.AddRange(CongViecBUS.DoUuTienList); cboUuTien.SelectedIndex = 1;
            cboCVTT = new ComboBox { Left = 120, Top = 220, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCVTT.Items.AddRange(CongViecBUS.TrangThaiList); cboCVTT.SelectedIndex = 0;
            clbThanhVien = new CheckedListBox { Left = 120, Top = 255, Width = 240, Height = 120, CheckOnClick = true };
            var btnThem = Btn("Thêm", 15, 390, Color.FromArgb(22, 163, 74), 80);
            var btnSua = Btn("Sửa", 100, 390, Color.FromArgb(234, 88, 12), 80);
            var btnXoa = Btn("Xóa", 185, 390, Color.FromArgb(220, 38, 38), 80);
            var btnMoi = Btn("Mới", 270, 390, Color.Gray, 80);
            pnlForm.Controls.AddRange(new Control[]
            {
                Lbl("Dự án:", 10, 10), cboCVDuAn, Lbl("Tên công việc:", 10, 45), txtTenTask, Lbl("Mô tả:", 10, 80), txtMoTa,
                Lbl("Hạn nộp:", 10, 150), dtpHan, Lbl("Độ ưu tiên:", 10, 185), cboUuTien, Lbl("Trạng thái:", 10, 220), cboCVTT,
                Lbl("Giao cho:", 10, 255), clbThanhVien, btnThem, btnSua, btnXoa, btnMoi
            });

            gridCV = Grid();
            gridCV.Dock = DockStyle.Fill;
            gridCV.SelectionChanged += (s, e) =>
            {
                if (gridCV.CurrentRow?.DataBoundItem is DataRowView r) FillTaskForm(r.Row);
            };
            gridCV.CellFormatting += (s, e) =>
            {
                if (gridCV.Columns[e.ColumnIndex].Name == "TrangThai" && e.Value != null)
                    e.CellStyle.ForeColor = StatusColor(e.Value.ToString());
            };

            kanban = new FlowLayoutPanel { Dock = DockStyle.Fill, Visible = false, AutoScroll = true, BackColor = Color.FromArgb(241, 245, 249), WrapContents = false };

            cboLocDuAn.SelectedIndexChanged += (s, e) => Try(LoadCongViec);
            cboLocTT.SelectedIndexChanged += (s, e) => Try(LoadCongViec);
            rdoList.CheckedChanged += (s, e) => Try(LoadCongViec);

            btnThem.Click += (s, e) => Try(() => { cvBus.Insert(ReadTask(), CheckedTV()); LoadCongViec(); Info("Đã thêm công việc."); });
            btnSua.Click += (s, e) => Try(() =>
            {
                if (selTask == 0) throw new Exception("Chọn công việc cần sửa.");
                var c = ReadTask(); c.MaTask = selTask; cvBus.Update(c, CheckedTV()); LoadCongViec(); Info("Đã cập nhật công việc.");
            });
            btnXoa.Click += (s, e) => Try(() =>
            {
                if (selTask == 0) throw new Exception("Chọn công việc cần xóa.");
                if (Confirm("Xóa công việc này?")) { cvBus.Delete(selTask); selTask = 0; LoadCongViec(); }
            });
            btnMoi.Click += (s, e) => { selTask = 0; txtTenTask.Clear(); txtMoTa.Clear(); for (int i = 0; i < clbThanhVien.Items.Count; i++) clbThanhVien.SetItemChecked(i, false); };
            btnNext.Click += (s, e) => Try(() =>
            {
                if (selTask == 0) throw new Exception("Chọn công việc cần chuyển trạng thái.");
                string next = cvBus.ChuyenTrangThaiTiepTheo(selTask, cboCVTT.SelectedItem.ToString());
                LoadCongViec();
                Info($"Đã chuyển công việc #{selTask} sang \"{next}\".");
            });

            tp.Controls.Add(gridCV);
            tp.Controls.Add(kanban);
            tp.Controls.Add(pnlForm);
            tp.Controls.Add(pnlLoc);
            return tp;
        }

        private static Color StatusColor(string s)
        {
            switch (s)
            {
                case "To Do": return Color.FromArgb(100, 116, 139);
                case "In Progress": return Color.FromArgb(37, 99, 235);
                case "Review": return Color.FromArgb(217, 119, 6);
                case "Done": return Color.FromArgb(22, 163, 74);
                default: return Color.Black;
            }
        }

        private void LoadComboCongViec()
        {
            DataTable da = duAnBus.GetAll();
            DataTable loc = da.Copy();
            DataRow all = loc.NewRow(); all["MaDuAn"] = 0; all["TenDuAn"] = "(Tất cả dự án)"; all["TrangThai"] = ""; loc.Rows.InsertAt(all, 0);
            int keep = cboLocDuAn.SelectedValue is int v ? v : 0;
            cboLocDuAn.DisplayMember = "TenDuAn"; cboLocDuAn.ValueMember = "MaDuAn"; cboLocDuAn.DataSource = loc;
            cboLocDuAn.SelectedValue = keep;
            cboCVDuAn.DisplayMember = "TenDuAn"; cboCVDuAn.ValueMember = "MaDuAn"; cboCVDuAn.DataSource = da;

            clbThanhVien.Items.Clear();
            foreach (DataRow r in tvBus.GetAll().Rows)
                clbThanhVien.Items.Add(new ListItem((int)r["MaThanhVien"], r["HoTen"].ToString()));
        }

        private void LoadCongViec()
        {
            int maDuAn = cboLocDuAn.SelectedValue is int v ? v : 0;
            string tt = cboLocTT.SelectedIndex <= 0 ? null : cboLocTT.SelectedItem.ToString();
            DataTable dt = cvBus.Filter(maDuAn, tt);

            gridCV.Visible = rdoList.Checked;
            kanban.Visible = !rdoList.Checked;
            gridCV.DataSource = dt;
            SetHeaders(gridCV, ("MaTask", "Mã"), ("TenDuAn", "Dự án"), ("TenTask", "Công việc"), ("HanNop", "Hạn nộp"),
                ("DoUuTien", "Ưu tiên"), ("TrangThai", "Trạng thái"), ("NguoiThucHien", "Người thực hiện"));
            if (gridCV.Columns.Contains("MaDuAn")) gridCV.Columns["MaDuAn"].Visible = false;
            if (gridCV.Columns.Contains("MoTa")) gridCV.Columns["MoTa"].Visible = false;

            if (!rdoList.Checked) BuildKanban(dt);
        }

        private void BuildKanban(DataTable dt)
        {
            kanban.SuspendLayout();
            kanban.Controls.Clear();
            foreach (string st in CongViecBUS.TrangThaiList)
            {
                var col = new FlowLayoutPanel
                {
                    Width = 200, Height = kanban.ClientSize.Height - 20, FlowDirection = FlowDirection.TopDown,
                    WrapContents = false, AutoScroll = true, BackColor = Color.FromArgb(226, 232, 240), Margin = new Padding(8)
                };
                var rows = dt.AsEnumerable().Where(r => r.Field<string>("TrangThai") == st).ToList();
                col.Controls.Add(new Label
                {
                    Text = $"{st} ({rows.Count})", Width = 185, Height = 30, ForeColor = Color.White, BackColor = StatusColor(st),
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter
                });
                foreach (DataRow r in rows)
                {
                    DateTime? han = r["HanNop"] == DBNull.Value ? (DateTime?)null : (DateTime)r["HanNop"];
                    bool tre = han.HasValue && han.Value < DateTime.Now && st != "Done";
                    var card = new Label
                    {
                        Width = 185, Height = 78, BackColor = tre ? Color.FromArgb(254, 226, 226) : Color.White,
                        BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(4), Cursor = Cursors.Hand,
                        Text = $"#{r["MaTask"]} {r["TenTask"]}\n{r["TenDuAn"]}\nHạn: {han:dd/MM/yyyy} | {r["DoUuTien"]}\n👤 {r["NguoiThucHien"]}"
                    };
                    DataRow row = r;
                    card.Click += (s, e) => FillTaskForm(row);
                    col.Controls.Add(card);
                }
                kanban.Controls.Add(col);
            }
            kanban.ResumeLayout();
        }

        private void FillTaskForm(DataRow r)
        {
            selTask = (int)r["MaTask"];
            cboCVDuAn.SelectedValue = (int)r["MaDuAn"];
            txtTenTask.Text = r["TenTask"].ToString();
            txtMoTa.Text = r["MoTa"].ToString();
            if (r["HanNop"] != DBNull.Value) dtpHan.Value = (DateTime)r["HanNop"];
            cboUuTien.SelectedItem = r["DoUuTien"].ToString();
            cboCVTT.SelectedItem = r["TrangThai"].ToString();
            var ds = cvBus.GetThanhVien(selTask);
            for (int i = 0; i < clbThanhVien.Items.Count; i++)
                clbThanhVien.SetItemChecked(i, ds.Contains(((ListItem)clbThanhVien.Items[i]).Id));
        }

        private CongViecDTO ReadTask() => new CongViecDTO
        {
            MaDuAn = cboCVDuAn.SelectedValue is int v ? v : 0,
            TenTask = txtTenTask.Text.Trim(),
            MoTa = txtMoTa.Text.Trim(),
            HanNop = dtpHan.Value.Date,
            DoUuTien = cboUuTien.SelectedItem?.ToString(),
            TrangThai = cboCVTT.SelectedItem?.ToString()
        };

        private int[] CheckedTV() => clbThanhVien.CheckedItems.Cast<ListItem>().Select(i => i.Id).ToArray();

        // ================= BÁO CÁO =================
        private TabPage BuildTabBaoCao()
        {
            var tp = new TabPage("Báo cáo & Cảnh báo") { BackColor = Color.White };
            var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 300 };
            gridTre = Grid(); gridTre.Dock = DockStyle.Fill;
            gridTre.DefaultCellStyle.ForeColor = Color.FromArgb(185, 28, 28);
            gridTienDo = Grid(); gridTienDo.Dock = DockStyle.Fill;
            split.Panel1.Controls.Add(gridTre);
            split.Panel1.Controls.Add(new Label { Text = "⚠ CÔNG VIỆC TRỄ HẠN (HanNop < hôm nay và chưa Done) — đọc bằng SqlDataReader", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(185, 28, 28) });
            split.Panel2.Controls.Add(gridTienDo);
            split.Panel2.Controls.Add(new Label { Text = "📊 TỶ LỆ % HOÀN THÀNH THEO DỰ ÁN — GROUP BY", Dock = DockStyle.Top, Height = 30, Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Primary });
            tp.Controls.Add(split);
            return tp;
        }

        private void LoadBaoCao()
        {
            gridTre.DataSource = bcBus.GetCongViecTreHan();
            SetHeaders(gridTre, ("MaTask", "Mã"), ("TenDuAn", "Dự án"), ("TenTask", "Công việc"), ("HanNop", "Hạn nộp"), ("SoNgayTre", "Số ngày trễ"), ("TrangThai", "Trạng thái"));
            gridTienDo.DataSource = bcBus.GetTienDoDuAn();
            SetHeaders(gridTienDo, ("MaDuAn", "Mã"), ("TenDuAn", "Dự án"), ("TongTask", "Tổng task"), ("DaXong", "Đã xong"), ("PhanTramHoanThanh", "% Hoàn thành"));
        }

        // ================= Helpers =================
        private static void SetHeaders(DataGridView g, params (string col, string header)[] map)
        {
            foreach (var m in map)
                if (g.Columns.Contains(m.col)) g.Columns[m.col].HeaderText = m.header;
        }

        private static void Try(Action a)
        {
            try { a(); } catch (Exception ex) { Error(ex); }
        }

        private class ListItem
        {
            public int Id { get; }
            public string Name { get; }
            public ListItem(int id, string name) { Id = id; Name = name; }
            public override string ToString() => Name;
        }
    }
}
