-- =============================================
-- BÀI 6.4: GarageDB - Quản lý garage & bảo dưỡng xe ô tô
-- =============================================
IF DB_ID('GarageDB') IS NULL CREATE DATABASE GarageDB;
GO
USE GarageDB;
GO
IF OBJECT_ID('ChiTietSuaChua') IS NOT NULL DROP TABLE ChiTietSuaChua;
IF OBJECT_ID('PhieuSuaChua') IS NOT NULL DROP TABLE PhieuSuaChua;
IF OBJECT_ID('PhuTung') IS NOT NULL DROP TABLE PhuTung;
IF OBJECT_ID('Xe') IS NOT NULL DROP TABLE Xe;
IF OBJECT_ID('KhachHang') IS NOT NULL DROP TABLE KhachHang;
GO
CREATE TABLE KhachHang (
    MaKH       INT IDENTITY(1,1) PRIMARY KEY,
    TenKH      NVARCHAR(100) NOT NULL,
    DienThoai  VARCHAR(15) UNIQUE,
    DiaChi     NVARCHAR(200)
);
CREATE TABLE Xe (
    BienSo      VARCHAR(20) PRIMARY KEY,
    MaKH        INT NOT NULL FOREIGN KEY REFERENCES KhachHang(MaKH),
    HieuXe      NVARCHAR(50),
    NamSanXuat  INT
);
CREATE TABLE PhuTung (
    MaPhuTung   INT IDENTITY(1,1) PRIMARY KEY,
    TenPhuTung  NVARCHAR(150) NOT NULL,
    DonGia      DECIMAL(18,2) NOT NULL DEFAULT 0,
    SoLuongTon  INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0)
);
CREATE TABLE PhieuSuaChua (
    MaPhieu         INT IDENTITY(1,1) PRIMARY KEY,
    BienSo          VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES Xe(BienSo),
    NgayNhan        DATETIME DEFAULT GETDATE(),
    NoiDungSua      NVARCHAR(MAX),
    TongTien        DECIMAL(18,2) DEFAULT 0,
    TrangThai       NVARCHAR(30) DEFAULT N'Đang sửa'
);
CREATE TABLE ChiTietSuaChua (
    MaPhieu    INT NOT NULL FOREIGN KEY REFERENCES PhieuSuaChua(MaPhieu),
    MaPhuTung  INT NOT NULL FOREIGN KEY REFERENCES PhuTung(MaPhuTung),
    SoLuong    INT NOT NULL CHECK (SoLuong > 0),
    TienCong   DECIMAL(18,2) DEFAULT 0,
    PRIMARY KEY (MaPhieu, MaPhuTung)
);
GO
INSERT INTO KhachHang(TenKH, DienThoai, DiaChi) VALUES
(N'Nguyễn Văn Hùng', '0901234567', N'Cầu Giấy, Hà Nội'),
(N'Trần Thị Lan', '0912345678', N'Đống Đa, Hà Nội'),
(N'Lê Quốc Bảo', '0987654321', N'Thanh Xuân, Hà Nội');
INSERT INTO Xe(BienSo, MaKH, HieuXe, NamSanXuat) VALUES
('30A-123.45', 1, N'Toyota', 2019),
('30G-678.90', 2, N'Honda', 2021),
('29H-555.66', 3, N'VinFast', 2023);
INSERT INTO PhuTung(TenPhuTung, DonGia, SoLuongTon) VALUES
(N'Dầu nhớt Castrol 4L', 650000, 40),
(N'Lọc gió động cơ', 280000, 25),
(N'Má phanh trước', 950000, 10),
(N'Bugi Denso', 120000, 60),
(N'Ắc quy GS 60Ah', 1850000, 3),
(N'Gạt mưa Bosch', 220000, 0);
INSERT INTO PhieuSuaChua(BienSo, NgayNhan, NoiDungSua, TongTien, TrangThai) VALUES
('30A-123.45', DATEADD(day,-10,GETDATE()), N'Bảo dưỡng định kỳ 20.000km', 1230000, N'Đã thanh toán'),
('30G-678.90', DATEADD(day,-2,GETDATE()), N'Thay má phanh', 1250000, N'Đã hoàn thành');
INSERT INTO ChiTietSuaChua(MaPhieu, MaPhuTung, SoLuong, TienCong) VALUES
(1, 1, 1, 150000), (1, 2, 1, 150000), (2, 3, 1, 300000);
GO
