USE master;
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
    MaNhanVien VARCHAR(20) PRIMARY KEY,
    TenNhanVien NVARCHAR(100) NOT NULL,
    GioiTinh NVARCHAR(10),
    NgaySinh DATE,
    DiaChi NVARCHAR(MAX),
    SoDienThoai VARCHAR(15),
    TenDangNhap VARCHAR(50) UNIQUE NOT NULL,
    MatKhau VARCHAR(100) NOT NULL,
    Quyen NVARCHAR(50),
    CONSTRAINT CK_NV_Tuoi CHECK (DATEDIFF(YEAR, NgaySinh, GETDATE()) >= 18),
    CONSTRAINT CK_NV_SDT CHECK (SoDienThoai NOT LIKE '%[^0-9]%' AND LEN(SoDienThoai) BETWEEN 10 AND 15)
);

CREATE TABLE KhachHang (
    MaKhachHang VARCHAR(20) PRIMARY KEY,
    TenKhachHang NVARCHAR(100) NOT NULL,
    DienThoai VARCHAR(15),
    Diem INT DEFAULT 0,
    CONSTRAINT CK_KH_SDT CHECK (DienThoai NOT LIKE '%[^0-9]%' AND LEN(DienThoai) BETWEEN 10 AND 15),
    CONSTRAINT CK_KH_Diem CHECK (Diem >= 0)
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
    GhiChu NVARCHAR(MAX),
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
    MaNhanVien VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    FOREIGN KEY (MaNCC) REFERENCES NhaCungCap(MaNCC),
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
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
    MaKhachHang VARCHAR(20),
    MaNhanVien VARCHAR(20),
    TongTien DECIMAL(18,2) DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CONSTRAINT CK_HD_TrangThai CHECK (TrangThai IN (N'Chưa thanh toán', N'Đã thanh toán', N'Đã hủy')),
    FOREIGN KEY (MaKhachHang) REFERENCES KhachHang(MaKhachHang),
    FOREIGN KEY (MaNhanVien) REFERENCES NhanVien(MaNhanVien)
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

-- =========================================================
-- 1. TRIGGER CHO BẢNG CHI TIẾT NHẬP HÀNG (GIỮ NGUYÊN - TỐT)
-- =========================================================
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

-- =========================================================
-- 2. TRIGGER CHI TIẾT BÁN HÀNG: INSERT, UPDATE, DELETE 
-- (Đã sửa lại cơ chế tính toán kho để tránh lỗi khi sửa giỏ hàng)
-- =========================================================
CREATE TRIGGER TRG_ChiTietHoaDon_AllActions ON ChiTietHoaDon AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;

    -- BƯỚC 1: HOÀN LẠI KHO SỐ LƯỢNG CŨ (NẾU LÀ HÀNH ĐỘNG UPDATE HOẶC DELETE)
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong
        FROM SanPham sp
        JOIN deleted d ON sp.MaSP = d.MaSP
        JOIN HoaDon hd ON d.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy'; -- Chỉ hoàn kho nếu hóa đơn chưa từng bị hủy trước đó
    END

    -- BƯỚC 2: TRỪ KHO THEO SỐ LƯỢNG MỚI (NẾU LÀ HÀNH ĐỘNG INSERT HOẶC UPDATE)
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- Kiểm tra xem sau khi tính toán kho mới, có sản phẩm nào bị âm kho không
        IF EXISTS (
            SELECT 1 FROM inserted i
            JOIN SanPham sp ON sp.MaSP = i.MaSP
            JOIN HoaDon hd ON i.MaHD = hd.MaHD
            WHERE sp.SoLuongTon < i.SoLuong AND hd.TrangThai <> N'Đã hủy'
        )
        BEGIN
            RAISERROR(N'Lỗi: Số lượng hàng tồn kho không đủ để thực hiện giao dịch!', 16, 1);
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

    -- BƯỚC 3: CẬP NHẬT LẠI TỔNG TIỀN TRÊN HÓA ĐƠN
    UPDATE hd
    SET hd.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaBan), 0) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD)
    FROM HoaDon hd
    WHERE hd.MaHD IN (SELECT MaHD FROM inserted UNION SELECT MaHD FROM deleted);
END;
GO

-- =========================================================
-- 3. TRIGGER MỚI: TỰ ĐỘNG HOÀN KHO KHI C# CẬP NHẬT TRẠNG THÁI "ĐÃ HỦY"
-- =========================================================
CREATE TRIGGER TRG_HoaDon_UpdateTrangThai ON HoaDon AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    -- Kiểm tra xem có hóa đơn nào vừa được chuyển trạng thái sang 'Đã hủy' hay không
    IF EXISTS (
        SELECT 1 FROM inserted i 
        JOIN deleted d ON i.MaHD = d.MaHD 
        WHERE i.TrangThai = N'Đã hủy' AND d.TrangThai <> N'Đã hủy'
    )
    BEGIN
        -- Tiến hành cộng trả lại toàn bộ số lượng sản phẩm của hóa đơn đó vào kho hàng
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + ct.SoLuong
        FROM SanPham sp
        JOIN ChiTietHoaDon ct ON sp.MaSP = ct.MaSP
        JOIN inserted i ON ct.MaHD = i.MaHD
        JOIN deleted d ON i.MaHD = d.MaHD
        WHERE i.TrangThai = N'Đã hủy' AND d.TrangThai <> N'Đã hủy';
    END
END;
GO


-- =========================================================
-- CÁC PHẦN VIEW, PROCEDURE VÀ DATA SEED (GIỮ NGUYÊN)
-- =========================================================
CREATE VIEW v_DoanhThuTheoThang AS
SELECT 
    YEAR(NgayLap) AS Nam,
    MONTH(NgayLap) AS Thang,
    COUNT(DISTINCT hd.MaHD) AS TongSoHoaDon,
    SUM(ct.SoLuong) AS TongSanPhamDaBan,
    SUM(hd.TongTien) AS TongDoanhThu
FROM HoaDon hd
JOIN ChiTietHoaDon ct ON hd.MaHD = ct.MaHD
WHERE hd.TrangThai = N'Đã thanh toán'
GROUP BY YEAR(NgayLap), MONTH(NgayLap);
GO

CREATE PROCEDURE sp_ThongKeDoanhThuTheoSanPham
    @TuNgay DATETIME,
    @DenNgay DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        lsp.TenLoai AS TenLoaiSanPham,
        sp.MaSP,
        sp.TenSP,
        sp.Size,
        sp.MauSac,
        SUM(cthd.SoLuong) AS SoLuongDaBan,
        SUM(cthd.SoLuong * cthd.GiaBan) AS DoanhThuThuVe
    FROM ChiTietHoaDon cthd
    JOIN HoaDon hd ON cthd.MaHD = hd.MaHD
    JOIN SanPham sp ON cthd.MaSP = sp.MaSP
    JOIN LoaiSanPham lsp ON sp.MaLoai = lsp.MaLoai
    WHERE hd.TrangThai = N'Đã thanh toán' 
      AND hd.NgayLap BETWEEN @TuNgay AND @DenNgay
    GROUP BY lsp.TenLoai, sp.MaSP, sp.TenSP, sp.Size, sp.MauSac
    ORDER BY SoLuongDaBan DESC;
END;
GO

-- SEED DATA LOẠI SẢN PHẨM
INSERT INTO LoaiSanPham (MaLoai, TenLoai) VALUES 
('LH01', N'Sneaker Thể Thao'), 
('LH02', N'Giày Tây Công Sở'), 
('LH03', N'Giày Chạy Bộ'),
('LH04', N'Giày Sandal Học Sinh'),
('LH05', N'Dép Thời Trang');

-- SEED DATA NHÀ CUNG CẤP
INSERT INTO NhaCungCap (MaNCC, TenNCC, DiaChi, SDT) VALUES 
('NCC01', N'Công ty TNHH Nike Việt Nam', N'KCN Amata, Biên Hòa, Đồng Nai', '02513891111'), 
('NCC02', N'Nhà phân phối Adidas Đông Nam Á', N'Tòa nhà Bitexco, Quận 1, HCM', '02838211222'),
('NCC03', N'Tổng kho Puma Miền Nam', N'KCN Tân Bình, Tân Phú, HCM', '0908123456'),
('NCC04', N'Công ty Cổ phần Biti''s Việt Nam', N'Chợ Lớn, Quận 6, HCM', '02838554900');

-- SEED DATA NHÂN VIÊN
INSERT INTO NhanVien (MaNhanVien, TenNhanVien, GioiTinh, NgaySinh, DiaChi, SoDienThoai, TenDangNhap, MatKhau, Quyen) VALUES 
('NV01', N'Nguyễn Quản Lý', N'Nam', '1990-05-15', N'Quận 1, HCM', '0912345678', 'quanly', '123456', N'Quản lý'),
('NV02', N'Trần Bán Hàng', N'Nữ', '1998-11-20', N'Quận 3, HCM', '0988888888', 'banhang1', '123456', N'Nhân viên'),
('NV03', N'Lê Thu Ngân', N'Nữ', '2001-02-25', N'Bình Thạnh, HCM', '0977777777', 'thungan', '123456', N'Nhân viên'),
('NV04', N'Phạm Kho Quỹ', N'Nam', '1995-07-12', N'Gò Vấp, HCM', '0966666666', 'khoquy', '123456', N'Nhân viên');

-- SEED DATA KHÁCH HÀNG
INSERT INTO KhachHang (MaKhachHang, TenKhachHang, DienThoai, Diem) VALUES 
('KH01', N'Khách Hàng Lẻ', '0000000000', 0),
('KH02', N'Nguyễn Đình Anh', '0901234567', 120),
('KH03', N'Trần Thị Bo', '0908889991', 50),
('KH04', N'Phạm Hồng Phúc', '0911223344', 85),
('KH05', N'Vũ Hoàng Long', '0933445566', 200);

-- SEED DATA SẢN PHẨM (Khởi tạo ban đầu tồn kho = 0)
INSERT INTO SanPham (MaSP, TenSP, MaLoai, MaNCC, Size, MauSac, GiaNhap, GiaBan, SoLuongTon, GhiChu) VALUES 
('SP01', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '40', N'Trắng', 1600000, 2800000, 0, N'Hàng bán chạy'),
('SP02', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '41', N'Trắng', 1600000, 2800000, 0, NULL),
('SP03', N'Nike Air Force 1 Black', 'LH01', 'NCC01', '42', N'Đen', 1650000, 2850000, 0, NULL),
('SP04', N'Adidas Ultraboost 22 Black', 'LH03', 'NCC02', '41', N'Đen', 2800000, 4500000, 0, N'Đế êm'),
('SP05', N'Adidas Ultraboost 22 Grey', 'LH03', 'NCC02', '42', N'Xám', 2850000, 4550000, 0, NULL),
('SP
