using System;
using System.Drawing;
using System.Windows.Forms;

namespace GarageManagement.GUI
{
    /// <summary>Các hàm dựng giao diện dùng chung.</summary>
    internal static class UiHelper
    {
        public static readonly Color Primary = Color.FromArgb(37, 99, 235);

        public static Label Lbl(string text, int x, int y, int w = 110) =>
            new Label { Text = text, Left = x, Top = y + 4, Width = w, AutoSize = false };

        public static Button Btn(string text, int x, int y, Color? color = null, int w = 100)
        {
            var b = new Button
            {
                Text = text, Left = x, Top = y, Width = w, Height = 32,
                FlatStyle = FlatStyle.Flat, BackColor = color ?? Primary, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        public static DataGridView Grid()
        {
            var g = new DataGridView
            {
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
                EnableHeadersVisualStyles = false
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Primary;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            return g;
        }

        public static void Error(Exception ex) =>
            MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static void Info(string msg) =>
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static bool Confirm(string msg) =>
            MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
