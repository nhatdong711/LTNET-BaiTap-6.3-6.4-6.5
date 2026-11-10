-- =============================================
-- BÀI 6.3: ProjectTaskDB - Quản lý công việc & tiến độ dự án
-- =============================================
IF DB_ID('ProjectTaskDB') IS NULL CREATE DATABASE ProjectTaskDB;
GO
USE ProjectTaskDB;
GO
IF OBJECT_ID('PhanCong') IS NOT NULL DROP TABLE PhanCong;
IF OBJECT_ID('CongViec') IS NOT NULL DROP TABLE CongViec;
IF OBJECT_ID('ThanhVien') IS NOT NULL DROP TABLE ThanhVien;
IF OBJECT_ID('DuAn') IS NOT NULL DROP TABLE DuAn;
GO
CREATE TABLE DuAn (
    MaDuAn      INT IDENTITY(1,1) PRIMARY KEY,
    TenDuAn     NVARCHAR(150) NOT NULL,
    NgayBatDau  DATETIME,
    NgayKetThuc DATETIME,
    TrangThai   NVARCHAR(50)
);
CREATE TABLE ThanhVien (
    MaThanhVien INT IDENTITY(1,1) PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       VARCHAR(100) NULL,
    ChuyenMon   NVARCHAR(100)
);
-- Email Unique (cho phép nhiều NULL)
CREATE UNIQUE INDEX UQ_ThanhVien_Email ON ThanhVien(Email) WHERE Email IS NOT NULL;
CREATE TABLE CongViec (
    MaTask    INT IDENTITY(1,1) PRIMARY KEY,
    MaDuAn    INT NOT NULL FOREIGN KEY REFERENCES DuAn(MaDuAn),
    TenTask   NVARCHAR(200) NOT NULL,
    MoTa      NVARCHAR(MAX),
    HanNop    DATETIME,
    DoUuTien  NVARCHAR(20),
    TrangThai NVARCHAR(50) NOT NULL DEFAULT 'To Do'
);
CREATE TABLE PhanCong (
    MaTask      INT NOT NULL FOREIGN KEY REFERENCES CongViec(MaTask),
    MaThanhVien INT NOT NULL FOREIGN KEY REFERENCES ThanhVien(MaThanhVien),
    NgayGiao    DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (MaTask, MaThanhVien)
);
GO
-- ===== Dữ liệu mẫu =====
INSERT INTO DuAn(TenDuAn, NgayBatDau, NgayKetThuc, TrangThai) VALUES
(N'Website bán hàng CellphoneS', DATEADD(day,-30,GETDATE()), DATEADD(day,30,GETDATE()), N'Đang chạy'),
(N'App quản lý quán cà phê The Kafe', DATEADD(day,-60,GETDATE()), DATEADD(day,-5,GETDATE()), N'Hoàn thành'),
(N'Hệ thống HRM nội bộ', DATEADD(day,-10,GETDATE()), DATEADD(day,60,GETDATE()), N'Tạm dừng');

INSERT INTO ThanhVien(HoTen, Email, ChuyenMon) VALUES
(N'Nguyễn Văn An', 'an.nv@gmail.com', N'Backend .NET'),
(N'Trần Thị Bình', 'binh.tt@gmail.com', N'Frontend'),
(N'Lê Minh Cường', 'cuong.lm@gmail.com', N'Tester'),
(N'Phạm Thu Dung', 'dung.pt@gmail.com', N'BA / PM');

INSERT INTO CongViec(MaDuAn, TenTask, MoTa, HanNop, DoUuTien, TrangThai) VALUES
(1, N'Thiết kế CSDL', N'Thiết kế ERD và script', DATEADD(day,-20,GETDATE()), N'Cao', 'Done'),
(1, N'API sản phẩm', N'CRUD sản phẩm', DATEADD(day,-2,GETDATE()), N'Cao', 'In Progress'),
(1, N'Giao diện giỏ hàng', N'UI giỏ hàng + thanh toán', DATEADD(day,5,GETDATE()), N'Trung bình', 'To Do'),
(1, N'Kiểm thử chức năng đặt hàng', NULL, DATEADD(day,-1,GETDATE()), N'Trung bình', 'Review'),
(2, N'Màn hình gọi món', NULL, DATEADD(day,-15,GETDATE()), N'Cao', 'Done'),
(2, N'Báo cáo doanh thu', NULL, DATEADD(day,-10,GETDATE()), N'Thấp', 'Done'),
(3, N'Phân tích yêu cầu HRM', NULL, DATEADD(day,-3,GETDATE()), N'Cao', 'To Do'),
(3, N'Module chấm công', NULL, DATEADD(day,20,GETDATE()), N'Trung bình', 'To Do');

INSERT INTO PhanCong(MaTask, MaThanhVien) VALUES (1,1),(1,4),(2,1),(3,2),(4,3),(5,2),(6,1),(7,4),(8,1),(8,2);
GO
