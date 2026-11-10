# BÀI 6.4: HỆ THỐNG QUẢN LÝ GARAGE & BẢO DƯỠNG XE Ô TÔ (GARAGE MANAGEMENT SYSTEM)

> Bài tập 6.4 — Môn Lập trình .NET (D19QTANM1)

## MỤC 1: THÔNG TIN SINH VIÊN

- **Họ và tên sinh viên:** Trần Nhật Đông
- **Mã sinh viên:** 24810320177
- **Lớp / Khóa:** D19QTANM1

## MỤC 2: HÌNH ẢNH KẾT QUẢ CÁC MÀN HÌNH CHỨC NĂNG

1. **Màn hình Chính / Tổng quan hệ thống:**
![Màn hình chính](images/01_main_screen.png)

2. **Màn hình Quản lý Danh mục (CRUD - Thêm/Sửa/Xóa):**
![Màn hình CRUD](images/02_crud_screen.png)

3. **Màn hình Thực thi Nghiệp vụ chính (Lập phiếu sửa chữa với SqlTransaction):**
![Màn hình Nghiệp vụ](images/03_transaction_screen.png)

4. **Màn hình Báo cáo - Thống kê:**
![Màn hình Thống kê](images/04_report_screen.png)

5. **Minh họa An toàn Dữ liệu (Phòng chống SQL Injection / Báo lỗi Transaction Rollback):**
![Minh họa Xử lý lỗi](images/05_error_handling.png)

## MỤC 3: HƯỚNG DẪN CÀI ĐẶT & CHẠY

**Yêu cầu:** Visual Studio 2022 trở lên (workload *.NET desktop development*), .NET 8 SDK, SQL Server (Developer/Express/LocalDB).

1. Clone repository và mở file `GarageManagement.sln` bằng Visual Studio.
2. **Tạo CSDL:** chương trình **tự động** tìm SQL Server (`.`, `.\SQLEXPRESS`, `(localdb)\MSSQLLocalDB`) và chạy `Database/ScriptDB.sql` ở lần chạy đầu tiên.
   Có thể chạy thủ công script `Database/ScriptDB.sql` trong SSMS nếu muốn.
3. Nếu cần, sửa chuỗi kết nối trong `GarageManagement/App.config`.
4. Nhấn **F5** để chạy (NuGet tự khôi phục gói `Microsoft.Data.SqlClient`, `System.Configuration.ConfigurationManager`).

## MỤC 4: KIẾN TRÚC 3-TIER

```
Bai6.4_Garage/
├── Database/
│   └── ScriptDB.sql          # Script khởi tạo CSDL SQL Server và chèn dữ liệu mẫu
├── GarageManagement/
│   ├── DTO/                  # KhachHangDTO, XeDTO, PhuTungDTO, PhieuSuaChuaDTO / ChiTietSuaChuaDTO
│   ├── DAL/                  # DBConnect, XeDAL, PhuTungDAL, PhieuSuaChuaDAL
│   ├── BUS/                  # XeBUS, PhuTungBUS, PhieuSuaChuaBUS
│   ├── GUI/                  # FrmMain (Tổng quan, Tiếp nhận xe, Kho phụ tùng, Lập phiếu, Thanh toán, Báo cáo)
│   ├── App.config / Program.cs
├── images/                   # Ảnh chụp màn hình
├── README.md
└── GarageManagement.sln
```

| Tầng | Vai trò |
|------|---------|
| **DTO** | Đối tượng truyền dữ liệu giữa các tầng |
| **DAL** | Truy vấn ADO.NET (SqlConnection, SqlCommand + SqlParameter, SqlDataReader, SqlDataAdapter, SqlTransaction) |
| **BUS** | Kiểm tra ràng buộc, tính toán nghiệp vụ |
| **GUI** | Giao diện WinForms, chỉ gọi tầng BUS |

## MỤC 5: CHỨC NĂNG ĐÃ HOÀN THÀNH
- [x] CSDL `GarageDB` 5 bảng: KhachHang, Xe, PhuTung, PhieuSuaChua, ChiTietSuaChua (+ dữ liệu mẫu)
- [x] Tra cứu xe theo biển số bằng **SqlParameter**; xe mới → nhập thông tin chủ xe và xe
- [x] Quản lý danh mục phụ tùng (CRUD), cập nhật đơn giá & số lượng tồn kho (tô đỏ khi sắp hết)
- [x] Lập phiếu sửa chữa: thêm nhiều phụ tùng + tiền công, tính tổng tiền tự động
- [x] Khi lưu phiếu, chạy **SqlTransaction**: (1) tạo PhieuSuaChua → (2) tạo ChiTietSuaChua → (3) trừ `SoLuongTon` → (4) thiếu hàng thì `trans.Rollback()` và báo lỗi
- [x] Tổng chi phí = Σ(Số lượng × Đơn giá) + Tiền công
- [x] Chuyển trạng thái: Đang sửa → Đã hoàn thành → **Đã thanh toán** và xuất hóa đơn
- [x] Báo cáo doanh thu theo tháng, danh sách phụ tùng sắp hết

> Gợi ý chụp ảnh lỗi (ảnh 05): lập phiếu với *Ắc quy GS 60Ah* số lượng 10 (kho còn 3) hoặc *Gạt mưa Bosch* (kho 0) → giao dịch bị Rollback.

