<<<<<<< Updated upstream
﻿USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = N'QLShopGiay')
BEGIN
    ALTER DATABASE QLShopGiay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLShopGiay;
END
GO

CREATE DATABASE QLShopGiay;
GO
USE QLShopGiay;
GO

--------------------------------------------------------------------------------
-- CẤU TRÚC BẢNG (THEO DANH SÁCH RÚT GỌN CỦA HUY)
--------------------------------------------------------------------------------

CREATE TABLE LoaiSanPham (
    MaLoai VARCHAR(10) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL
);

CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(10) PRIMARY KEY,
    TenNCC NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(MAX),
    SDT VARCHAR(15),
    CONSTRAINT CK_NCC_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15)
);

CREATE TABLE NhanVien (
    MaNV VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    SDT VARCHAR(15),
    Email VARCHAR(100) UNIQUE,
    ChucVu NVARCHAR(50),
    MatKhau VARBINARY(MAX),
    Salt UNIQUEIDENTIFIER DEFAULT NEWID(),
    TrangThai BIT DEFAULT 1,
    CONSTRAINT CK_NV_Tuoi CHECK (DATEDIFF(YEAR, NgaySinh, GETDATE()) >= 18),
    CONSTRAINT CK_NV_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15),
    CONSTRAINT CK_Email_Format CHECK (Email LIKE '%_@_%._%')
);

CREATE TABLE KhachHang (
    MaKH VARCHAR(20) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(MAX),
    CONSTRAINT CK_KH_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15)
);

CREATE TABLE SanPham (
    MaSP VARCHAR(20) PRIMARY KEY,
    TenSP NVARCHAR(200) NOT NULL,
    MaLoai VARCHAR(10),
    MaNCC VARCHAR(10),
    Size VARCHAR(10),
    MauSac NVARCHAR(30),
    GiaNhap DECIMAL(18,2) DEFAULT 0,
    GiaBan DECIMAL(18,2) DEFAULT 0,
    SoLuongTon INT DEFAULT 0,
    CONSTRAINT CK_GiaNhap CHECK (GiaNhap >= 0),
    CONSTRAINT CK_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_SoLuongTon CHECK (SoLuongTon >= 0),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

CREATE TABLE HoaDonNhap (
    MaHDN VARCHAR(20) PRIMARY KEY,
    NgayNhap DATETIME DEFAULT GETDATE(),
    MaNCC VARCHAR(10),
    MaNV VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietHoaDonNhap (
    MaHDN VARCHAR(20),
    MaSP VARCHAR(20),
    SoLuong INT CHECK (SoLuong > 0),
    GiaNhap DECIMAL(18,2) CHECK (GiaNhap >= 0),
    PRIMARY KEY (MaHDN, MaSP),
    FOREIGN KEY (MaHDN) REFERENCES HoaDonNhap(MaHDN),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

CREATE TABLE HoaDon (
    MaHD VARCHAR(20) PRIMARY KEY,
    NgayLap DATETIME DEFAULT GETDATE(),
    MaKH VARCHAR(20),
    MaNV VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CONSTRAINT CK_HD_TrangThai CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán', N'Đã hủy')),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietHoaDon (
    MaHD VARCHAR(20),
    MaSP VARCHAR(20),
    SoLuong INT CHECK (SoLuong > 0),
    GiaBan DECIMAL(18,2) CHECK (GiaBan >= 0),
    PRIMARY KEY (MaHD, MaSP),
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

--------------------------------------------------------------------------------
-- CÁC TRIGGER TỰ ĐỘNG ĐỒNG BỘ DỮ LIỆU (ĐÃ FIX LỖI)
--------------------------------------------------------------------------------
CREATE TRIGGER TRG_UpdateKho_KhiNhapHang ON ChiTietHoaDonNhap AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong
    FROM SanPham sp
    JOIN inserted i ON sp.MaSP = i.MaSP;

    UPDATE hdn
    SET hdn.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaNhap), 0) FROM ChiTietHoaDonNhap WHERE MaHDN = hdn.MaHDN)
    FROM HoaDonNhap hdn
    WHERE hdn.MaHDN IN (SELECT DISTINCT MaHDN FROM inserted);
END;
GO

CREATE TRIGGER TRG_ChiTietHoaDon_AllActions ON ChiTietHoaDon AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Xử lý hoàn kho khi xóa/sửa dữ liệu (Bảng deleted)
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong
        FROM SanPham sp
        JOIN deleted d ON sp.MaSP = d.MaSP
        JOIN HoaDon hd ON d.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy';
    END

    -- 2. Xử lý kiểm tra hàng tồn và trừ kho khi thêm/sửa dữ liệu (Bảng inserted)
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (
            SELECT 1 FROM inserted i
            JOIN SanPham sp ON sp.MaSP = i.MaSP
            JOIN HoaDon hd ON i.MaHD = hd.MaHD
            WHERE sp.SoLuongTon < i.SoLuong AND hd.TrangThai <> N'Đã hủy'
        )
        BEGIN
            RAISERROR(N'Lỗi: Số lượng hàng tồn kho không đủ!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong
        FROM SanPham sp
        JOIN inserted i ON sp.MaSP = i.MaSP
        JOIN HoaDon hd ON i.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy';
    END

    -- 3. Cập nhật lại tổng tiền cho hóa đơn (ĐÃ ĐỔI TÊN BIẾN MaHDEN THÀNH MaHD CHUẨN)
    UPDATE hd
    SET hd.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaBan), 0) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD)
    FROM HoaDon hd
    WHERE hd.MaHD IN (SELECT MaHD FROM inserted UNION SELECT MaHD FROM deleted);
END;
GO

--------------------------------------------------------------------------------
-- HỆ THỐNG DỮ LIỆU MẪU QUY MÔ LỚN (TEST DATA)
--------------------------------------------------------------------------------

-- 1. LOẠI SẢN PHẨM
INSERT INTO LoaiSanPham (MaLoai, TenLoai) VALUES 
('LH01', N'Sneaker Thể Thao'), 
('LH02', N'Giày Tây Công Sở'), 
('LH03', N'Giày Chạy Bộ'),
('LH04', N'Giày Sandal Học Sinh'),
('LH05', N'Dép Thời Trang');

-- 2. NHÀ CUNG CẤP
INSERT INTO NhaCungCap (MaNCC, TenNCC, DiaChi, SDT) VALUES 
('NCC01', N'Công ty TNHH Nike Việt Nam', N'KCN Amata, Biên Hòa, Đồng Nai', '02513891111'), 
('NCC02', N'Nhà phân phối Adidas Đông Nam Á', N'Tòa nhà Bitexco, Quận 1, HCM', '02838211222'),
('NCC03', N'Tổng kho Puma Miền Nam', N'KCN Tân Bình, Tân Phú, HCM', '0908123456'),
('NCC04', N'Công ty Cổ phần Biti''s Việt Nam', N'Chợ Lớn, Quận 6, HCM', '02838554900');

-- 3. NHÂN VIÊN
INSERT INTO NhanVien (MaNV, HoTen, NgaySinh, SDT, Email, ChucVu) VALUES 
('NV01', N'Nguyễn Quản Lý', '1990-05-15', '0912345678', 'quanly@shopgiay.com', N'Quản lý cửa hàng'),
('NV02', N'Trần Bán Hàng', '1998-11-20', '0988888888', 'banhang1@shopgiay.com', N'Nhân viên bán hàng'),
('NV03', N'Lê Thu Ngân', '2001-02-25', '0977777777', 'thungan@shopgiay.com', N'Nhân viên thu ngân'),
('NV04', N'Phạm Kho Quỹ', '1995-07-12', '0966666666', 'khoquy@shopgiay.com', N'Nhân viên kho');

-- 4. KHÁCH HÀNG
INSERT INTO KhachHang (MaKH, TenKH, SDT, DiaChi) VALUES 
('KH01', N'Khách Hàng Lẻ', '0000000000', N'Mua tại quầy'),
('KH02', N'Nguyễn Đình Anh', '0901234567', N'Quận Bình Thạnh, HCM'),
('KH03', N'Trần Thị Bo', '0908889991', N'Quận 3, HCM'),
('KH04', N'Phạm Hồng Phúc', '0911223344', N'Thành phố Thủ Đức, HCM'),
('KH05', N'Vũ Hoàng Long', '0933445566', N'Quận Đống Đa, Hà Nội');

-- 5. SẢN PHẨM (Ban đầu khởi tạo tồn kho = 0)
INSERT INTO SanPham (MaSP, TenSP, MaLoai, MaNCC, Size, MauSac, GiaNhap, GiaBan, SoLuongTon) VALUES 
('SP01', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '40', N'Trắng', 1600000, 2800000, 0),
('SP02', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '41', N'Trắng', 1600000, 2800000, 0),
('SP03', N'Nike Air Force 1 Black', 'LH01', 'NCC01', '42', N'Đen', 1650000, 2850000, 0),
('SP04', N'Adidas Ultraboost 22 Black', 'LH03', 'NCC02', '41', N'Đen', 2800000, 4500000, 0),
('SP05', N'Adidas Ultraboost 22 Grey', 'LH03', 'NCC02', '42', N'Xám', 2850000, 4550000, 0),
('SP06', N'Puma Suede Classic Red', 'LH01', 'NCC03', '40', N'Đỏ', 1100000, 1900000, 0),
('SP07', N'Puma Suede Classic Blue', 'LH01', 'NCC03', '41', N'Xanh Dương', 1100000, 1900000, 0),
('SP08', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '39', N'Đen', 650000, 1100000, 0),
('SP09', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '40', N'Xanh Rêu', 650000, 1100000, 0),
('SP10', N'Giày Tây Oxford Premium', 'LH02', 'NCC04', '41', N'Nâu Đất', 900000, 1500000, 0);

-- 6. TIẾN HÀNH NHẬP HÀNG (Kích hoạt Trigger nhập kho tự động cộng hàng tồn)
-- Đơn nhập 01: Nhập hàng Nike
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN01', '2026-05-01 09:00:00', 'NCC01', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN01', 'SP01', 50, 1600000),
('HDN01', 'SP02', 50, 1600000),
('HDN01', 'SP03', 30, 1650000);

-- Đơn nhập 02: Nhập hàng Adidas và Puma
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN02', '2026-05-05 14:30:00', 'NCC02', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN02', 'SP04', 40, 2800000),
('HDN02', 'SP05', 40, 2850000),
('HDN02', 'SP06', 20, 1100000);

-- Đơn nhập 03: Nhập hàng Biti's
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN03', '2026-05-10 10:15:00', 'NCC04', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN03', 'SP08', 60, 650000),
('HDN03', 'SP09', 60, 650000),
('HDN03', 'SP10', 30, 900000);

-- 7. TIẾN HÀNH XUẤT BÁN HÀNG (Kích hoạt Trigger trừ kho và tự động tính tổng tiền hóa đơn)
-- Hóa đơn 01: Khách hàng lẻ tại quầy
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD01', '2026-05-12 11:00:00', 'KH01', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD01', 'SP01', 2, 2800000),
('HD01', 'SP08', 1, 1100000);

-- Hóa đơn 02: Khách hàng Nguyễn Đình Anh mua đơn lớn
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD02', '2026-05-15 19:30:00', 'KH02', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD02', 'SP04', 5, 4500000),
('HD02', 'SP05', 2, 4550000),
('HD02', 'SP10', 3, 1500000);

-- Hóa đơn 03: Đơn hàng đặt trước chưa trả tiền
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD03', '2026-05-18 08:45:00', 'KH04', 'NV03', N'Chưa thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD03', 'SP02', 1, 2800000),
('HD03', 'SP09', 4, 1100000);

-- Hóa đơn 04: Đơn hàng bị hủy (Hệ thống tự động cộng trả lại hàng vào kho)
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD04', '2026-05-20 15:00:00', 'KH03', 'NV02', N'Đã hủy');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD04', 'SP03', 5, 2850000);
GO

--------------------------------------------------------------------------------
-- KIỂM TRA KẾT QUẢ TRUY VẤN SAU KHI CHẠY (QUICK TEST)
--------------------------------------------------------------------------------
SELECT * FROM SanPham;
SELECT * FROM HoaDonNhap;
=======
﻿USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = N'QLShopGiay')
BEGIN
    ALTER DATABASE QLShopGiay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLShopGiay;
END
GO

CREATE DATABASE QLShopGiay;
GO
USE QLShopGiay;
GO

--------------------------------------------------------------------------------
-- CẤU TRÚC BẢNG (THEO DANH SÁCH RÚT GỌN CỦA HUY)
--------------------------------------------------------------------------------

CREATE TABLE LoaiSanPham (
    MaLoai VARCHAR(10) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL
);

CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(10) PRIMARY KEY,
    TenNCC NVARCHAR(100) NOT NULL,
    DiaChi NVARCHAR(MAX),
    SDT VARCHAR(15),
    CONSTRAINT CK_NCC_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15)
);

CREATE TABLE NhanVien (
    MaNV VARCHAR(20) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    SDT VARCHAR(15),
    Email VARCHAR(100) UNIQUE,
    ChucVu NVARCHAR(50),
    MatKhau VARBINARY(MAX),
    Salt UNIQUEIDENTIFIER DEFAULT NEWID(),
    TrangThai BIT DEFAULT 1,
    CONSTRAINT CK_NV_Tuoi CHECK (DATEDIFF(YEAR, NgaySinh, GETDATE()) >= 18),
    CONSTRAINT CK_NV_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15),
    CONSTRAINT CK_Email_Format CHECK (Email LIKE '%_@_%._%')
);

CREATE TABLE KhachHang (
    MaKH VARCHAR(20) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(MAX),
    CONSTRAINT CK_KH_SDT CHECK (SDT NOT LIKE '%[^0-9]%' AND LEN(SDT) BETWEEN 10 AND 15)
);

CREATE TABLE SanPham (
    MaSP VARCHAR(20) PRIMARY KEY,
    TenSP NVARCHAR(200) NOT NULL,
    MaLoai VARCHAR(10),
    MaNCC VARCHAR(10),
    Size VARCHAR(10),
    MauSac NVARCHAR(30),
    GiaNhap DECIMAL(18,2) DEFAULT 0,
    GiaBan DECIMAL(18,2) DEFAULT 0,
    SoLuongTon INT DEFAULT 0,
    CONSTRAINT CK_GiaNhap CHECK (GiaNhap >= 0),
    CONSTRAINT CK_GiaBan CHECK (GiaBan >= 0),
    CONSTRAINT CK_SoLuongTon CHECK (SoLuongTon >= 0),
    FOREIGN KEY (MaLoai) REFERENCES LoaiSanPham(MaLoai),
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC)
);

CREATE TABLE HoaDonNhap (
    MaHDN VARCHAR(20) PRIMARY KEY,
    NgayNhap DATETIME DEFAULT GETDATE(),
    MaNCC VARCHAR(10),
    MaNV VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietHoaDonNhap (
    MaHDN VARCHAR(20),
    MaSP VARCHAR(20),
    SoLuong INT CHECK (SoLuong > 0),
    GiaNhap DECIMAL(18,2) CHECK (GiaNhap >= 0),
    PRIMARY KEY (MaHDN, MaSP),
    FOREIGN KEY (MaHDN) REFERENCES HoaDonNhap(MaHDN),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);

CREATE TABLE HoaDon (
    MaHD VARCHAR(20) PRIMARY KEY,
    NgayLap DATETIME DEFAULT GETDATE(),
    MaKH VARCHAR(20),
    MaNV VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CONSTRAINT CK_HD_TrangThai CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán', N'Đã hủy')),
    FOREIGN KEY (MaKH) REFERENCES KhachHang(MaKH),
    FOREIGN KEY (MaNV) REFERENCES NhanVien(MaNV)
);

CREATE TABLE ChiTietHoaDon (
    MaHD VARCHAR(20),
    MaSP VARCHAR(20),
    SoLuong INT CHECK (SoLuong > 0),
    GiaBan DECIMAL(18,2) CHECK (GiaBan >= 0),
    PRIMARY KEY (MaHD, MaSP),
    FOREIGN KEY (MaHD) REFERENCES HoaDon(MaHD),
    FOREIGN KEY (MaSP) REFERENCES SanPham(MaSP)
);
GO

--------------------------------------------------------------------------------
-- CÁC TRIGGER TỰ ĐỘNG ĐỒNG BỘ DỮ LIỆU (ĐÃ FIX LỖI)
--------------------------------------------------------------------------------
CREATE TRIGGER TRG_UpdateKho_KhiNhapHang ON ChiTietHoaDonNhap AFTER INSERT AS
BEGIN
    SET NOCOUNT ON;
    UPDATE sp
    SET sp.SoLuongTon = sp.SoLuongTon + i.SoLuong
    FROM SanPham sp
    JOIN inserted i ON sp.MaSP = i.MaSP;

    UPDATE hdn
    SET hdn.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaNhap), 0) FROM ChiTietHoaDonNhap WHERE MaHDN = hdn.MaHDN)
    FROM HoaDonNhap hdn
    WHERE hdn.MaHDN IN (SELECT DISTINCT MaHDN FROM inserted);
END;
GO

CREATE TRIGGER TRG_ChiTietHoaDon_AllActions ON ChiTietHoaDon AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Xử lý hoàn kho khi xóa/sửa dữ liệu (Bảng deleted)
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong
        FROM SanPham sp
        JOIN deleted d ON sp.MaSP = d.MaSP
        JOIN HoaDon hd ON d.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy';
    END

    -- 2. Xử lý kiểm tra hàng tồn và trừ kho khi thêm/sửa dữ liệu (Bảng inserted)
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        IF EXISTS (
            SELECT 1 FROM inserted i
            JOIN SanPham sp ON sp.MaSP = i.MaSP
            JOIN HoaDon hd ON i.MaHD = hd.MaHD
            WHERE sp.SoLuongTon < i.SoLuong AND hd.TrangThai <> N'Đã hủy'
        )
        BEGIN
            RAISERROR(N'Lỗi: Số lượng hàng tồn kho không đủ!', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END

        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon - i.SoLuong
        FROM SanPham sp
        JOIN inserted i ON sp.MaSP = i.MaSP
        JOIN HoaDon hd ON i.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy';
    END

    -- 3. Cập nhật lại tổng tiền cho hóa đơn (ĐÃ ĐỔI TÊN BIẾN MaHDEN THÀNH MaHD CHUẨN)
    UPDATE hd
    SET hd.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaBan), 0) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD)
    FROM HoaDon hd
    WHERE hd.MaHD IN (SELECT MaHD FROM inserted UNION SELECT MaHD FROM deleted);
END;
GO

--------------------------------------------------------------------------------
-- HỆ THỐNG DỮ LIỆU MẪU QUY MÔ LỚN (TEST DATA)
--------------------------------------------------------------------------------

-- 1. LOẠI SẢN PHẨM
INSERT INTO LoaiSanPham (MaLoai, TenLoai) VALUES 
('LH01', N'Sneaker Thể Thao'), 
('LH02', N'Giày Tây Công Sở'), 
('LH03', N'Giày Chạy Bộ'),
('LH04', N'Giày Sandal Học Sinh'),
('LH05', N'Dép Thời Trang');

-- 2. NHÀ CUNG CẤP
INSERT INTO NhaCungCap (MaNCC, TenNCC, DiaChi, SDT) VALUES 
('NCC01', N'Công ty TNHH Nike Việt Nam', N'KCN Amata, Biên Hòa, Đồng Nai', '02513891111'), 
('NCC02', N'Nhà phân phối Adidas Đông Nam Á', N'Tòa nhà Bitexco, Quận 1, HCM', '02838211222'),
('NCC03', N'Tổng kho Puma Miền Nam', N'KCN Tân Bình, Tân Phú, HCM', '0908123456'),
('NCC04', N'Công ty Cổ phần Biti''s Việt Nam', N'Chợ Lớn, Quận 6, HCM', '02838554900');

-- 3. NHÂN VIÊN
INSERT INTO NhanVien (MaNV, HoTen, NgaySinh, SDT, Email, ChucVu) VALUES 
('NV01', N'Nguyễn Quản Lý', '1990-05-15', '0912345678', 'quanly@shopgiay.com', N'Quản lý cửa hàng'),
('NV02', N'Trần Bán Hàng', '1998-11-20', '0988888888', 'banhang1@shopgiay.com', N'Nhân viên bán hàng'),
('NV03', N'Lê Thu Ngân', '2001-02-25', '0977777777', 'thungan@shopgiay.com', N'Nhân viên thu ngân'),
('NV04', N'Phạm Kho Quỹ', '1995-07-12', '0966666666', 'khoquy@shopgiay.com', N'Nhân viên kho');

-- 4. KHÁCH HÀNG
INSERT INTO KhachHang (MaKH, TenKH, SDT, DiaChi) VALUES 
('KH01', N'Khách Hàng Lẻ', '0000000000', N'Mua tại quầy'),
('KH02', N'Nguyễn Đình Anh', '0901234567', N'Quận Bình Thạnh, HCM'),
('KH03', N'Trần Thị Bo', '0908889991', N'Quận 3, HCM'),
('KH04', N'Phạm Hồng Phúc', '0911223344', N'Thành phố Thủ Đức, HCM'),
('KH05', N'Vũ Hoàng Long', '0933445566', N'Quận Đống Đa, Hà Nội');

-- 5. SẢN PHẨM (Ban đầu khởi tạo tồn kho = 0)
INSERT INTO SanPham (MaSP, TenSP, MaLoai, MaNCC, Size, MauSac, GiaNhap, GiaBan, SoLuongTon) VALUES 
('SP01', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '40', N'Trắng', 1600000, 2800000, 0),
('SP02', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '41', N'Trắng', 1600000, 2800000, 0),
('SP03', N'Nike Air Force 1 Black', 'LH01', 'NCC01', '42', N'Đen', 1650000, 2850000, 0),
('SP04', N'Adidas Ultraboost 22 Black', 'LH03', 'NCC02', '41', N'Đen', 2800000, 4500000, 0),
('SP05', N'Adidas Ultraboost 22 Grey', 'LH03', 'NCC02', '42', N'Xám', 2850000, 4550000, 0),
('SP06', N'Puma Suede Classic Red', 'LH01', 'NCC03', '40', N'Đỏ', 1100000, 1900000, 0),
('SP07', N'Puma Suede Classic Blue', 'LH01', 'NCC03', '41', N'Xanh Dương', 1100000, 1900000, 0),
('SP08', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '39', N'Đen', 650000, 1100000, 0),
('SP09', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '40', N'Xanh Rêu', 650000, 1100000, 0),
('SP10', N'Giày Tây Oxford Premium', 'LH02', 'NCC04', '41', N'Nâu Đất', 900000, 1500000, 0);

-- 6. TIẾN HÀNH NHẬP HÀNG (Kích hoạt Trigger nhập kho tự động cộng hàng tồn)
-- Đơn nhập 01: Nhập hàng Nike
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN01', '2026-05-01 09:00:00', 'NCC01', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN01', 'SP01', 50, 1600000),
('HDN01', 'SP02', 50, 1600000),
('HDN01', 'SP03', 30, 1650000);

-- Đơn nhập 02: Nhập hàng Adidas và Puma
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN02', '2026-05-05 14:30:00', 'NCC02', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN02', 'SP04', 40, 2800000),
('HDN02', 'SP05', 40, 2850000),
('HDN02', 'SP06', 20, 1100000);

-- Đơn nhập 03: Nhập hàng Biti's
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNV) VALUES ('HDN03', '2026-05-10 10:15:00', 'NCC04', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN03', 'SP08', 60, 650000),
('HDN03', 'SP09', 60, 650000),
('HDN03', 'SP10', 30, 900000);

-- 7. TIẾN HÀNH XUẤT BÁN HÀNG (Kích hoạt Trigger trừ kho và tự động tính tổng tiền hóa đơn)
-- Hóa đơn 01: Khách hàng lẻ tại quầy
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD01', '2026-05-12 11:00:00', 'KH01', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD01', 'SP01', 2, 2800000),
('HD01', 'SP08', 1, 1100000);

-- Hóa đơn 02: Khách hàng Nguyễn Đình Anh mua đơn lớn
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD02', '2026-05-15 19:30:00', 'KH02', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD02', 'SP04', 5, 4500000),
('HD02', 'SP05', 2, 4550000),
('HD02', 'SP10', 3, 1500000);

-- Hóa đơn 03: Đơn hàng đặt trước chưa trả tiền
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD03', '2026-05-18 08:45:00', 'KH04', 'NV03', N'Chưa thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD03', 'SP02', 1, 2800000),
('HD03', 'SP09', 4, 1100000);

-- Hóa đơn 04: Đơn hàng bị hủy (Hệ thống tự động cộng trả lại hàng vào kho)
INSERT INTO HoaDon (MaHD, NgayLap, MaKH, MaNV, TrangThai) VALUES ('HD04', '2026-05-20 15:00:00', 'KH03', 'NV02', N'Đã hủy');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD04', 'SP03', 5, 2850000);
GO

--------------------------------------------------------------------------------
-- KIỂM TRA KẾT QUẢ TRUY VẤN SAU KHI CHẠY (QUICK TEST)
--------------------------------------------------------------------------------
SELECT * FROM SanPham;
SELECT * FROM HoaDonNhap;
>>>>>>> Stashed changes
SELECT * FROM HoaDon;