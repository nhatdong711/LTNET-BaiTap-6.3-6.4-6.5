# BÀI 6.5: HỆ THỐNG QUẢN LÝ ĐẶT PHÒNG & DỊCH VỤ KARAOKE (KARAOKE MANAGEMENT SYSTEM)

> Bài tập 6.5 — Môn Lập trình .NET (D19QTANM1)

## MỤC 1: THÔNG TIN SINH VIÊN

- **Họ và tên sinh viên:** [Điền họ tên]
- **Mã sinh viên:** [Điền MSV]
- **Lớp / Khóa:** D19QTANM1

## MỤC 2: HÌNH ẢNH KẾT QUẢ CÁC MÀN HÌNH CHỨC NĂNG

1. **Màn hình Chính / Tổng quan hệ thống:**
![Màn hình chính](images/01_main_screen.png)

2. **Màn hình Quản lý Danh mục (CRUD - Thêm/Sửa/Xóa):**
![Màn hình CRUD](images/02_crud_screen.png)

3. **Màn hình Thực thi Nghiệp vụ chính (Mở phòng, gọi dịch vụ, thanh toán bằng SqlTransaction):**
![Màn hình Nghiệp vụ](images/03_transaction_screen.png)

4. **Màn hình Báo cáo - Thống kê:**
![Màn hình Thống kê](images/04_report_screen.png)

5. **Minh họa An toàn Dữ liệu (Phòng chống SQL Injection / Báo lỗi Transaction Rollback):**
![Minh họa Xử lý lỗi](images/05_error_handling.png)

## MỤC 3: HƯỚNG DẪN CÀI ĐẶT & CHẠY

**Yêu cầu:** Visual Studio 2022 trở lên (workload *.NET desktop development*), .NET 8 SDK, SQL Server (Developer/Express/LocalDB).

1. Clone repository và mở file `KaraokeManagement.sln` bằng Visual Studio.
2. **Tạo CSDL:** chương trình **tự động** tìm SQL Server (`.`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`) và chạy `Database/ScriptDB.sql` ở lần chạy đầu tiên.
   Có thể chạy thủ công script `Database/ScriptDB.sql` trong SSMS nếu muốn.
3. Nếu cần, sửa chuỗi kết nối trong `KaraokeManagement/App.config`.
4. Nhấn **F5** để chạy (NuGet tự khôi phục gói `Microsoft.Data.SqlClient`, `System.Configuration.ConfigurationManager`).

## MỤC 4: KIẾN TRÚC 3-TIER

```
Bai6.5_Karaoke/
├── Database/
│   └── ScriptDB.sql          # Script khởi tạo CSDL SQL Server và chèn dữ liệu mẫu
├── KaraokeManagement/
│   ├── DTO/                  # PhongHatDTO, DichVuDTO, HoaDonDTO
│   ├── DAL/                  # DBConnect, PhongHatDAL, DichVuDAL (SqlDataAdapter), HoaDonDAL
│   ├── BUS/                  # PhongHatBUS, DichVuBUS, HoaDonBUS
│   ├── GUI/                  # FrmMain (Sơ đồ phòng, Quản lý dịch vụ, Báo cáo doanh thu)
│   ├── App.config / Program.cs
├── images/                   # Ảnh chụp màn hình
├── README.md
└── KaraokeManagement.sln
```

| Tầng | Vai trò |
|------|---------|
| **DTO** | Đối tượng truyền dữ liệu giữa các tầng |
| **DAL** | Truy vấn ADO.NET (SqlConnection, SqlCommand + SqlParameter, SqlDataReader, SqlDataAdapter, SqlTransaction) |
| **BUS** | Kiểm tra ràng buộc, tính toán nghiệp vụ |
| **GUI** | Giao diện WinForms, chỉ gọi tầng BUS |

## MỤC 5: CHỨC NĂNG ĐÃ HOÀN THÀNH
- [x] CSDL `KaraokeDB` 4 bảng: PhongHat, DichVu, HoaDon, ChiTietDichVu (+ dữ liệu mẫu)
- [x] **Sơ đồ phòng trực quan (Visual Grid)**: mỗi phòng là 1 Button, phòng "Trống" màu xanh, "Đang hát" màu đỏ
- [x] Mở phòng: tạo HoaDon mới với `GioVao = GETDATE()`, phòng chuyển sang "Đang hát"
- [x] Gọi dịch vụ / đồ uống theo **mô hình ngắt kết nối**: danh sách dịch vụ nạp bằng `SqlDataAdapter` + `DataTable` lên DataGridView; quản lý dịch vụ sửa trên DataTable rồi `adapter.Update()`
- [x] Đồng hồ thời gian hát và tạm tính tiền theo thời gian thực
- [x] Thanh toán: thời gian hát = GioRa − GioVao; TongTienPhong = Số giờ × GiaTheoGio (làm tròn 15 phút, tối thiểu 1 giờ)
- [x] **SqlTransaction**: (1) cập nhật GioRa, TongTienPhong, TongTienDichVu, TrangThai = 1 → (2) trừ TonKho → (3) phòng về "Trống" → (4) `trans.Commit()`; lỗi thì `Rollback()`
- [x] Báo cáo doanh thu theo ngày (GROUP BY) và lịch sử hóa đơn

> Gợi ý chụp ảnh lỗi (ảnh 05): kho *Đậu phộng* chỉ còn 2 — gọi món vượt tồn kho sẽ bị chặn, giao dịch thanh toán thiếu hàng sẽ bị Rollback.

