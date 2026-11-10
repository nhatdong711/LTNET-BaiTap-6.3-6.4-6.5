-- =============================================
-- BÀI 6.5: KaraokeDB - Quản lý đặt phòng & dịch vụ Karaoke
-- =============================================
IF DB_ID('KaraokeDB') IS NULL CREATE DATABASE KaraokeDB;
GO
USE KaraokeDB;
GO
IF OBJECT_ID('ChiTietDichVu') IS NOT NULL DROP TABLE ChiTietDichVu;
IF OBJECT_ID('HoaDon') IS NOT NULL DROP TABLE HoaDon;
IF OBJECT_ID('DichVu') IS NOT NULL DROP TABLE DichVu;
IF OBJECT_ID('PhongHat') IS NOT NULL DROP TABLE PhongHat;
GO
CREATE TABLE PhongHat (
    MaPhong     INT IDENTITY(1,1) PRIMARY KEY,
    TenPhong    NVARCHAR(50) NOT NULL,
    LoaiPhong   NVARCHAR(30),
    GiaTheoGio  DECIMAL(18,2) NOT NULL,
    TrangThai   NVARCHAR(20) NOT NULL DEFAULT N'Trống'
);
CREATE TABLE DichVu (
    MaDV    INT IDENTITY(1,1) PRIMARY KEY,
    TenDV   NVARCHAR(100) NOT NULL,
    DonGia  DECIMAL(18,2) NOT NULL,
    TonKho  INT NOT NULL DEFAULT 0 CHECK (TonKho >= 0)
);
CREATE TABLE HoaDon (
    MaHD            INT IDENTITY(1,1) PRIMARY KEY,
    MaPhong         INT NOT NULL FOREIGN KEY REFERENCES PhongHat(MaPhong),
    GioVao          DATETIME NOT NULL,
    GioRa           DATETIME NULL,
    TongTienPhong   DECIMAL(18,2) DEFAULT 0,
    TongTienDichVu  DECIMAL(18,2) DEFAULT 0,
    TrangThai       BIT NOT NULL DEFAULT 0   -- 0: Chưa thanh toán, 1: Đã thanh toán
);
CREATE TABLE ChiTietDichVu (
    MaHD     INT NOT NULL FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaDV     INT NOT NULL FOREIGN KEY REFERENCES DichVu(MaDV),
    SoLuong  INT NOT NULL CHECK (SoLuong > 0),
    DonGia   DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHD, MaDV)
);
GO
INSERT INTO PhongHat(TenPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES
(N'Phòng VIP 01', N'VIP', 350000, N'Trống'),
(N'Phòng VIP 02', N'VIP', 350000, N'Trống'),
(N'Phòng Thường 01', N'Thường', 150000, N'Trống'),
(N'Phòng Thường 02', N'Thường', 150000, N'Trống'),
(N'Phòng Thường 03', N'Thường', 150000, N'Trống'),
(N'Phòng Thường 04', N'Thường', 150000, N'Trống'),
(N'Phòng VIP 03', N'VIP', 400000, N'Trống'),
(N'Phòng Thường 05', N'Thường', 180000, N'Trống');
INSERT INTO DichVu(TenDV, DonGia, TonKho) VALUES
(N'Bia Heineken', 35000, 200),
(N'Bia Tiger', 30000, 150),
(N'Nước ngọt Coca', 20000, 100),
(N'Trái cây dĩa', 150000, 20),
(N'Khô bò', 80000, 30),
(N'Đậu phộng', 30000, 2);
-- 2 phòng đang hát + 2 hóa đơn cũ đã thanh toán
INSERT INTO HoaDon(MaPhong, GioVao, GioRa, TongTienPhong, TongTienDichVu, TrangThai) VALUES
(1, DATEADD(day,-1,DATEADD(hour,-3,GETDATE())), DATEADD(day,-1,GETDATE()), 1050000, 500000, 1),
(3, DATEADD(day,-2,DATEADD(hour,-2,GETDATE())), DATEADD(day,-2,GETDATE()), 300000, 160000, 1),
(2, DATEADD(minute,-95,GETDATE()), NULL, 0, 0, 0),
(4, DATEADD(minute,-40,GETDATE()), NULL, 0, 0, 0);
INSERT INTO ChiTietDichVu(MaHD, MaDV, SoLuong, DonGia) VALUES
(1,1,10,35000),(1,4,1,150000),(2,3,5,20000),(2,6,2,30000),(3,1,6,35000),(3,5,1,80000);
UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong IN (2,4);
GO
