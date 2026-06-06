USE master;
GO

-- 1. XÓA CƠ SỞ DỮ LIỆU CŨ NẾU ĐÃ TỒN TẠI
IF EXISTS (SELECT * FROM sys.databases WHERE name = N'QLShopGiay')
BEGIN
    ALTER DATABASE QLShopGiay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLShopGiay;
END
GO

-- 2. KHỞI TẠO CƠ SỞ DỮ LIỆU MỚI
CREATE DATABASE QLShopGiay;
GO
USE QLShopGiay;
GO

-- =========================================================
-- 3. KHỞI TẠO CÁC BẢNG DỮ LIỆU (TABLES)
-- =========================================================

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
-- 4. KHỞI TẠO CÁC TRIGGER TỰ ĐỘNG TÍNH TOÁN VÀ QUẢN LÝ KHO
-- =========================================================

-- Trigger cộng kho khi nhập hàng
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

-- Trigger trừ kho khi thêm/sửa/xóa chi tiết hóa đơn bán hàng
CREATE TRIGGER TRG_ChiTietHoaDon_AllActions ON ChiTietHoaDon AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;

    -- Hoàn lại kho số lượng cũ khi sửa hoặc xóa sản phẩm trong giỏ
    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + d.SoLuong
        FROM SanPham sp
        JOIN deleted d ON sp.MaSP = d.MaSP
        JOIN HoaDon hd ON d.MaHD = hd.MaHD
        WHERE hd.TrangThai <> N'Đã hủy';
    END

    -- Trừ kho theo số lượng mới khi thêm mới hoặc sửa số lượng tăng lên
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        -- Bẫy lỗi nếu kho không đủ hàng
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

    -- Cập nhật lại tổng tiền hóa đơn bán
    UPDATE hd
    SET hd.TongTien = (SELECT COALESCE(SUM(SoLuong * GiaBan), 0) FROM ChiTietHoaDon WHERE MaHD = hd.MaHD)
    FROM HoaDon hd
    WHERE hd.MaHD IN (SELECT MaHD FROM inserted UNION SELECT MaHD FROM deleted);
END;
GO

-- Trigger tự động hoàn kho khi cập nhật trạng thái hóa đơn thành 'Đã hủy' từ Code C#
CREATE TRIGGER TRG_HoaDon_UpdateTrangThai ON HoaDon AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 FROM inserted i 
        JOIN deleted d ON i.MaHD = d.MaHD 
        WHERE i.TrangThai = N'Đã hủy' AND d.TrangThai <> N'Đã hủy'
    )
    BEGIN
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
-- 5. KHỞI TẠO VIEW VÀ STORED PROCEDURE
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

-- =========================================================
-- 6. CHÈN DỮ LIỆU MẪU (SEED DATA) - ĐÃ FIX CHUỖI NHÁY ĐƠN TRÁNH LỖI MSG 105
-- =========================================================

INSERT INTO LoaiSanPham (MaLoai, TenLoai) VALUES 
('LH01', N'Sneaker Thể Thao'), 
('LH02', N'Giày Tây Công Sở'), 
('LH03', N'Giày Chạy Bộ'),
('LH04', N'Giày Sandal Học Sinh'),
('LH05', N'Dép Thời Trang');

INSERT INTO NhaCungCap (MaNCC, TenNCC, DiaChi, SDT) VALUES 
('NCC01', N'Công ty TNHH Nike Việt Nam', N'KCN Amata, Biên Hòa, Đồng Nai', '02513891111'), 
('NCC02', N'Nhà phân phối Adidas Đông Nam Á', N'Tòa nhà Bitexco, Quận 1, HCM', '02838211222'),
('NCC03', N'Tổng kho Puma Miền Nam', N'KCN Tân Bình, Tân Phú, HCM', '0908123456'),
('NCC04', N'Công ty Cổ phần Biti''s Việt Nam', N'Chợ Lớn, Quận 6, HCM', '02838554900');

INSERT INTO NhanVien (MaNhanVien, TenNhanVien, GioiTinh, NgaySinh, DiaChi, SoDienThoai, TenDangNhap, MatKhau, Quyen) VALUES 
('NV01', N'Nguyễn Quản Lý', N'Nam', '1990-05-15', N'Quận 1, HCM', '0912345678', 'quanly', '123456', N'Quản lý'),
('NV02', N'Trần Bán Hàng', N'Nữ', '1998-11-20', N'Quận 3, HCM', '0988888888', 'banhang1', '123456', N'Nhân viên'),
('NV03', N'Lê Thu Ngân', N'Nữ', '2001-02-25', N'Bình Thạnh, HCM', '0977777777', 'thungan', '123456', N'Nhân viên'),
('NV04', N'Phạm Kho Quỹ', N'Nam', '1995-07-12', N'Gò Vấp, HCM', '0966666666', 'khoquy', '123456', N'Nhân viên');

INSERT INTO KhachHang (MaKhachHang, TenKhachHang, DienThoai, Diem) VALUES 
('KH01', N'Khách Hàng Lẻ', '0000000000', 0),
('KH02', N'Nguyễn Đình Anh', '0901234567', 120),
('KH03', N'Trần Thị Bo', '0908889991', 50),
('KH04', N'Phạm Hồng Phúc', '0911223344', 85),
('KH05', N'Vũ Hoàng Long', '0933445566', 200);

-- Lưu ý: Tên thương hiệu Biti''s đã sử dụng cặp nháy đơn chuẩn của T-SQL
INSERT INTO SanPham (MaSP, TenSP, MaLoai, MaNCC, Size, MauSac, GiaNhap, GiaBan, SoLuongTon, GhiChu) VALUES 
('SP01', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '40', N'Trắng', 1600000, 2800000, 0, N'Hàng bán chạy'),
('SP02', N'Nike Air Force 1 All White', 'LH01', 'NCC01', '41', N'Trắng', 1600000, 2800000, 0, NULL),
('SP03', N'Nike Air Force 1 Black', 'LH01', 'NCC01', '42', N'Đen', 1650000, 2850000, 0, NULL),
('SP04', N'Adidas Ultraboost 22 Black', 'LH03', 'NCC02', '41', N'Đen', 2800000, 4500000, 0, N'Đế êm'),
('SP05', N'Adidas Ultraboost 22 Grey', 'LH03', 'NCC02', '42', N'Xám', 2850000, 4550000, 0, NULL),
('SP06', N'Puma Suede Classic Red', 'LH01', 'NCC03', '40', N'Đỏ', 1100000, 1900000, 0, NULL),
('SP07', N'Puma Suede Classic Blue', 'LH01', 'NCC03', '41', N'Xanh Dương', 1100000, 1900000, 0, NULL),
('SP08', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '39', N'Đen', 650000, 1100000, 0, N'Phiên bản giới hạn'),
('SP09', N'Biti''s Hunter X Dune', 'LH01', 'NCC04', '40', N'Xanh Rêu', 650000, 1100000, 0, NULL),
('SP10', N'Giày Tây Oxford Premium', 'LH02', 'NCC04', '41', N'Nâu Đất', 900000, 1500000, 0, NULL);

-- Thực hiện Nhập hàng (Số lượng tồn kho tự tăng lên thông qua trigger)
INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNhanVien) VALUES ('HDN01', '2026-05-01 09:00:00', 'NCC01', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN01', 'SP01', 50, 1600000),
('HDN01', 'SP02', 50, 1600000),
('HDN01', 'SP03', 30, 1650000);

INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNhanVien) VALUES ('HDN02', '2026-05-05 14:30:00', 'NCC02', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN02', 'SP04', 40, 2800000),
('HDN02', 'SP05', 40, 2850000),
('HDN02', 'SP06', 20, 1100000);

INSERT INTO HoaDonNhap (MaHDN, NgayNhap, MaNCC, MaNhanVien) VALUES ('HDN03', '2026-05-10 10:15:00', 'NCC04', 'NV04');
INSERT INTO ChiTietHoaDonNhap (MaHDN, MaSP, SoLuong, GiaNhap) VALUES 
('HDN03', 'SP08', 60, 650000),
('HDN03', 'SP09', 60, 650000),
('HDN03', 'SP10', 30, 900000);

-- Thực hiện bán hàng (Kho tự trừ đi lượng tương ứng qua trigger)
INSERT INTO HoaDon (MaHD, NgayLap, MaKhachHang, MaNhanVien, TrangThai) VALUES ('HD01', '2026-05-12 11:00:00', 'KH01', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD01', 'SP01', 2, 2800000),
('HD01', 'SP08', 1, 1100000);

INSERT INTO HoaDon (MaHD, NgayLap, MaKhachHang, MaNhanVien, TrangThai) VALUES ('HD02', '2026-05-15 19:30:00', 'KH02', 'NV02', N'Đã thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD02', 'SP04', 5, 4500000),
('HD02', 'SP05', 2, 4550000),
('HD02', 'SP10', 3, 1500000);

INSERT INTO HoaDon (MaHD, NgayLap, MaKhachHang, MaNhanVien, TrangThai) VALUES ('HD03', '2026-05-18 08:45:00', 'KH04', 'NV03', N'Chưa thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES 
('HD03', 'SP02', 1, 2800000),
('HD03', 'SP09', 4, 1100000);

-- Thử nghiệm đơn hàng nháp sau đó hủy (Đơn HD04 được tạo -> kho trừ 5 -> sau đó cập nhật Hủy -> kho được hoàn lại 5 cái)
INSERT INTO HoaDon (MaHD, NgayLap, MaKhachHang, MaNhanVien, TrangThai) VALUES ('HD04', '2026-05-20 15:00:00', 'KH03', 'NV02', N'Chưa thanh toán');
INSERT INTO ChiTietHoaDon (MaHD, MaSP, SoLuong, GiaBan) VALUES ('HD04', 'SP03', 5, 2850000);
UPDATE HoaDon SET TrangThai = N'Đã hủy' WHERE MaHD = 'HD04';
GO

-- =========================================================
-- 7. TRUY VẤN KIỂM TRA DỮ LIỆU ĐẦU RA 
-- =========================================================
SELECT * FROM SanPham;
SELECT * FROM KhachHang;
SELECT * FROM NhanVien;
SELECT * FROM v_DoanhThuTheoThang;
EXEC sp_ThongKeDoanhThuTheoSanPham '2026-05-01', '2026-05-31';
GO
-- Cập nhật mật khẩu cho tất cả nhân viên test
UPDATE NhanVien 
SET MatKhau = '123456'
WHERE MaNhanVien IN ('NV01', 'NV02', 'NV03', 'NV04');
 
-- Kiểm tra kết quả
SELECT 
    MaNhanVien, 
    TenNhanVien, 
    TenDangNhap, 
    MatKhau, 
    Quyen
FROM NhanVien
WHERE MaNhanVien IN ('NV01', 'NV02', 'NV03', 'NV04');
-- =========================================================
-- THÊM BẢNG PHƯƠNG THỨC THANH TOÁN
-- =========================================================

USE QLShopGiay;
GO

-- 1. TẠO BẢNG PHƯƠNG THỨC THANH TOÁN
CREATE TABLE PhuongThucThanhToan (
    MaPhuongThuc VARCHAR(10) PRIMARY KEY,
    TenPhuongThuc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(MAX),
    TrangThai BIT DEFAULT 1  -- 1: Hoạt động, 0: Ngừng hoạt động
);
GO

-- 2. CẬP NHẬT BẢNG HÓAĐƠN - THÊMKHÓA NGOÀI PHƯƠNG THỨC THANH TOÁN
-- Thêm cột mới vào bảng HoaDon
ALTER TABLE HoaDon
ADD MaPhuongThuc VARCHAR(10) DEFAULT 'PT01';
GO

-- Thêm ràng buộc khóa ngoài
ALTER TABLE HoaDon
ADD CONSTRAINT FK_HoaDon_PhuongThuc 
FOREIGN KEY (MaPhuongThuc) REFERENCES PhuongThucThanhToan(MaPhuongThuc);
GO

-- 3. CHÈN DỮ LIỆU PHƯƠNG THỨC THANH TOÁN
INSERT INTO PhuongThucThanhToan (MaPhuongThuc, TenPhuongThuc, MoTa, TrangThai) VALUES
('PT01', N'Tiền Mặt', N'Thanh toán bằng tiền mặt trực tiếp tại quầy', 1),
('PT02', N'Thẻ Tín Dụng', N'Thanh toán bằng thẻ tín dụng (Visa, Mastercard, etc)', 1),
('PT03', N'Thẻ Ghi Nợ', N'Thanh toán bằng thẻ ghi nợ', 1),
('PT04', N'Chuyển Khoản Ngân Hàng', N'Thanh toán qua chuyển khoản ngân hàng', 1),
('PT05', N'Ví Điện Tử Momo', N'Thanh toán qua ứng dụng Momo', 1),
('PT06', N'Ví Điện Tử ZaloPay', N'Thanh toán qua ứng dụng ZaloPay', 1),
('PT07', N'QR Code (NAPAS)', N'Thanh toán bằng quét mã QR', 1),
('PT08', N'Thanh Toán Sau', N'Ghi nợ, thanh toán sau', 0);
GO

-- 4. CẬP NHẬT DỮ LIỆU HÓA ĐƠN CŨ VỚI PHƯƠNG THỨC THANH TOÁN MẶC ĐỊNH
UPDATE HoaDon SET MaPhuongThuc = 'PT01' WHERE MaPhuongThuc IS NULL;
GO

-- 5. KIỂM TRA DỮ LIỆU
SELECT * FROM PhuongThucThanhToan;
GO

SELECT 
    hd.MaHD, 
    hd.NgayLap,
    kh.TenKhachHang,
    nv.TenNhanVien,
    hd.TongTien,
    pt.TenPhuongThuc,
    hd.TrangThai
FROM HoaDon hd
LEFT JOIN KhachHang kh ON hd.MaKhachHang = kh.MaKhachHang
LEFT JOIN NhanVien nv ON hd.MaNhanVien = nv.MaNhanVien
LEFT JOIN PhuongThucThanhToan pt ON hd.MaPhuongThuc = pt.MaPhuongThuc
ORDER BY hd.NgayLap DESC;
GO

-- 6. TẠO VIEW MỚI - THỐNG KÊ DOANH THU THEO PHƯƠNG THỨC THANH TOÁN
CREATE VIEW v_DoanhThuTheoPhuongThuc AS
SELECT 
    pt.TenPhuongThuc,
    COUNT(hd.MaHD) AS SoLanThanhToan,
    SUM(hd.TongTien) AS TongDoanhThu,
    AVG(hd.TongTien) AS GiaTriTrungBinh
FROM HoaDon hd
JOIN PhuongThucThanhToan pt ON hd.MaPhuongThuc = pt.MaPhuongThuc
WHERE hd.TrangThai = N'Đã thanh toán'
GROUP BY pt.TenPhuongThuc, pt.MaPhuongThuc
ORDER BY TongDoanhThu DESC;
GO

-- 7. TẠO VIEW - THEO DÕI PHƯƠNG THỨC THANH TOÁN THEO THÁNG
CREATE VIEW v_DoanhThuPhuongThucTheoThang AS
SELECT 
    YEAR(hd.NgayLap) AS Nam,
    MONTH(hd.NgayLap) AS Thang,
    pt.TenPhuongThuc,
    COUNT(hd.MaHD) AS SoLuongHoaDon,
    SUM(hd.TongTien) AS TongDoanhThu
FROM HoaDon hd
JOIN PhuongThucThanhToan pt ON hd.MaPhuongThuc = pt.MaPhuongThuc
WHERE hd.TrangThai = N'Đã thanh toán'
GROUP BY YEAR(hd.NgayLap), MONTH(hd.NgayLap), pt.TenPhuongThuc, pt.MaPhuongThuc;
GO

-- 8. KIỂM TRA VIEW
SELECT * FROM v_DoanhThuTheoPhuongThuc;
SELECT * FROM v_DoanhThuPhuongThucTheoThang;
GO

