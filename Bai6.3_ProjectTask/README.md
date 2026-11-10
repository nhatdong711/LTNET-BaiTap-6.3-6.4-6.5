# BÀI 6.3: HỆ THỐNG QUẢN LÝ CÔNG VIỆC & TIẾN ĐỘ DỰ ÁN (PROJECT & TASK MANAGEMENT)

> Bài tập 6.3 — Môn Lập trình .NET (D19QTANM1)

## MỤC 1: THÔNG TIN SINH VIÊN

- **Họ và tên sinh viên:** Trần Nhật Đông
- **Mã sinh viên:** 24810320177
- **Lớp / Khóa:** D19QTANM1

## MỤC 2: HÌNH ẢNH KẾT QUẢ CÁC MÀN HÌNH CHỨC NĂNG

1. **Màn hình Chính / Tổng quan hệ thống:**
![Màn hình chính](images/01_main_screen.png)

2. **Màn hình Quản lý Danh mục (CRUD - Thêm/Sửa/Xóa):**
![Màn hình CRUD](images/02_crud_screen.png)

3. **Màn hình Thực thi Nghiệp vụ chính (Chuyển trạng thái công việc / Kanban):**
![Màn hình Nghiệp vụ](images/03_transaction_screen.png)

4. **Màn hình Báo cáo - Thống kê:**
![Màn hình Thống kê](images/04_report_screen.png)

5. **Minh họa An toàn Dữ liệu (Phòng chống SQL Injection / Báo lỗi Transaction Rollback):**
![Minh họa Xử lý lỗi](images/05_error_handling.png)

## MỤC 3: HƯỚNG DẪN CÀI ĐẶT & CHẠY

**Yêu cầu:** Visual Studio 2022 trở lên (workload *.NET desktop development*), .NET 8 SDK, SQL Server (Developer/Express/LocalDB).

1. Clone repository và mở file `ProjectTaskManagement.sln` bằng Visual Studio.
2. **Tạo CSDL:** chương trình **tự động** tìm SQL Server (`.`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`) và chạy `Database/ScriptDB.sql` ở lần chạy đầu tiên.
   Có thể chạy thủ công script `Database/ScriptDB.sql` trong SSMS nếu muốn.
3. Nếu cần, sửa chuỗi kết nối trong `ProjectTaskManagement/App.config`.
4. Nhấn **F5** để chạy (NuGet tự khôi phục gói `Microsoft.Data.SqlClient`, `System.Configuration.ConfigurationManager`).

## MỤC 4: KIẾN TRÚC 3-TIER

```
Bai6.3_ProjectTask/
├── Database/
│   └── ScriptDB.sql          # Script khởi tạo CSDL SQL Server và chèn dữ liệu mẫu
├── ProjectTaskManagement/
│   ├── DTO/                  # DuAnDTO, ThanhVienDTO, CongViecDTO, PhanCongDTO
│   ├── DAL/                  # DBConnect, DuAnDAL, ThanhVienDAL, CongViecDAL, PhanCongDAL, BaoCaoDAL
│   ├── BUS/                  # DuAnBUS, ThanhVienBUS, CongViecBUS, BaoCaoBUS
│   ├── GUI/                  # FrmMain (Tổng quan, Dự án, Thành viên, Công việc List/Kanban, Báo cáo)
│   ├── App.config / Program.cs
├── images/                   # Ảnh chụp màn hình
├── README.md
└── ProjectTaskManagement.sln
```

| Tầng | Vai trò |
|------|---------|
| **DTO** | Đối tượng truyền dữ liệu giữa các tầng |
| **DAL** | Truy vấn ADO.NET (SqlConnection, SqlCommand + SqlParameter, SqlDataReader, SqlDataAdapter, SqlTransaction) |
| **BUS** | Kiểm tra ràng buộc, tính toán nghiệp vụ |
| **GUI** | Giao diện WinForms, chỉ gọi tầng BUS |

## MỤC 5: CHỨC NĂNG ĐÃ HOÀN THÀNH
- [x] CSDL `ProjectTaskDB` 4 bảng: DuAn, ThanhVien, CongViec, PhanCong (+ dữ liệu mẫu)
- [x] Thêm / sửa / xóa Dự án, Thành viên, Công việc con thuộc dự án
- [x] Giao việc: một task cho nhiều thành viên (bảng PhanCong, CheckedListBox)
- [x] Lọc danh sách task theo Dự án và Trạng thái (To Do / In Progress / Review / Done) bằng **Parameterized SQL**
- [x] Giao diện **List view** (DataGridView) và **Kanban view** (4 cột theo trạng thái)
- [x] Chuyển trạng thái công việc (To Do → In Progress → Review → Done), cập nhật CSDL bằng Parameterized SQL Update
- [x] Cảnh báo trễ hạn: dùng **SqlDataReader** lấy các task có `HanNop < hôm nay` và `TrangThai != 'Done'` (tô đỏ trên Kanban)
- [x] Thống kê **% hoàn thành** của từng dự án bằng **GROUP BY** (và `ExecuteScalar` cho 1 dự án)

