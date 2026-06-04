using QuanLyShopGiay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyShopGiay.Helpers;

namespace QuanLyShopGiay.Services
{
    public class TaiKhoanService
    {
        // 1. READ: Chỉ lấy những tài khoản chưa bị xóa (IsDeleted == false)
        public List<TAI_KHOAN> GetAll()
        {
            using (var db = new QuanLyShopGiayEntities())
            {
                return db.TAI_KHOAN
                         .Where(x => x.IsDeleted == false)
                         .ToList();
            }
        }

        // 2. CREATE: Khi thêm mới, mặc định IsDeleted phải là false
        public ServiceResult Insert(TAI_KHOAN tk, string matKhau)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    // Validate: Mã tài khoản đã tồn tại chưa (kể cả đã bị khoá)
                    if (db.TAI_KHOAN.Any(x => x.MaTK == tk.MaTK))
                        return new ServiceResult(false, $"Mã tài khoản \"{tk.MaTK}\" đã tồn tại!");

                    // Validate: Tên đăng nhập đã bị trùng chưa (kể cả đã bị khoá)
                    if (db.TAI_KHOAN.Any(x => x.TenDangNhap == tk.TenDangNhap))
                        return new ServiceResult(false, $"Tên đăng nhập \"{tk.TenDangNhap}\" đã được sử dụng!");

                    Guid newSalt = Guid.NewGuid();
                    byte[] hashedBytes = PasswordHelper.HashPassword(matKhau, newSalt);

                    tk.Salt = newSalt;
                    tk.MatKhau = hashedBytes;
                    tk.IsDeleted = false;

                    db.TAI_KHOAN.Add(tk);
                    db.SaveChanges();
                    return new ServiceResult(true, "Tạo tài khoản thành công!");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Lỗi: " + ex.Message);
            }
        }

        // 3. UPDATE: Đảm bảo chỉ update tài khoản chưa bị xóa
        public ServiceResult Update(TAI_KHOAN tk)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    var existing = db.TAI_KHOAN.FirstOrDefault(x => x.MaTK == tk.MaTK && x.IsDeleted == false);
                    if (existing == null) return new ServiceResult(false, "Không tìm thấy tài khoản!");

                    existing.MaNV = tk.MaNV;
                    existing.TenDangNhap = tk.TenDangNhap;
                    existing.MaVT = tk.MaVT;

                    db.SaveChanges();
                    return new ServiceResult(true, "Cập nhật thành công!");
                }
            }
            catch (Exception ex) { return new ServiceResult(false, ex.Message); }
        }

        // 4. DELETE: Bây giờ là Soft Delete (Không dùng db.Remove)
        public ServiceResult Delete(string maTK)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    var tk = db.TAI_KHOAN.FirstOrDefault(x => x.MaTK == maTK && x.IsDeleted == false);

                    if (tk == null)
                        return new ServiceResult(false, "Tài khoản không tồn tại hoặc đã bị khóa!");

                    // CHỈ ĐỔI TRẠNG THÁI, KHÔNG XÓA DÒNG
                    tk.IsDeleted = true;

                    db.SaveChanges();
                    return new ServiceResult(true, "Đã khóa tài khoản thành công!");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Lỗi khi khóa tài khoản: " + ex.Message);
            }
        }
    }
}