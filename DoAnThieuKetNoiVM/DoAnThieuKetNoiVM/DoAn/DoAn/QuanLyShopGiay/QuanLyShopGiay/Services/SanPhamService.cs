using QuanLyShopGiay.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuanLyShopGiay.Services
{
    public class SanPhamService
    {
        // 1. READ
        public List<SAN_PHAM> GetAll()
        {
            using (var db = new QuanLyShopGiayEntities())
            {
                return db.SAN_PHAM
                         .Where(x => x.IsDeleted == false)
                         .ToList();
            }
        }

        // 2. CREATE
        public ServiceResult Insert(SAN_PHAM sp)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    // Validate dữ liệu
                    if (string.IsNullOrWhiteSpace(sp.MaSP))
                        return new ServiceResult(false, "Mã sản phẩm không được để trống!");

                    if (string.IsNullOrWhiteSpace(sp.TenSP))
                        return new ServiceResult(false, "Tên sản phẩm không được để trống!");

                    if (sp.GiaBan <= 0)
                        return new ServiceResult(false, "Giá bán phải lớn hơn 0!");

                    // Kiểm tra trùng mã
                    bool trungMa = db.SAN_PHAM.Any(x => x.MaSP == sp.MaSP);

                    if (trungMa)
                        return new ServiceResult(false, "Mã sản phẩm đã tồn tại!");

                    // Soft Delete mặc định
                    sp.IsDeleted = false;

                    db.SAN_PHAM.Add(sp);
                    db.SaveChanges();

                    return new ServiceResult(true, "Thêm sản phẩm thành công!");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Lỗi thêm sản phẩm: " + ex.Message);
            }
        }

        // 3. UPDATE
        public ServiceResult Update(SAN_PHAM sp)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    var existing = db.SAN_PHAM
                                     .FirstOrDefault(x =>
                                         x.MaSP == sp.MaSP &&
                                         x.IsDeleted == false);

                    if (existing == null)
                        return new ServiceResult(false, "Không tìm thấy sản phẩm!");

                    // Validate
                    if (string.IsNullOrWhiteSpace(sp.TenSP))
                        return new ServiceResult(false, "Tên sản phẩm không hợp lệ!");

                    if (sp.GiaBan <= 0)
                        return new ServiceResult(false, "Giá bán phải lớn hơn 0!");

                    // Update field
                    existing.TenSP = sp.TenSP;
                    existing.MaLoai = sp.MaLoai;
                    existing.MaNSX = sp.MaNSX;
                    existing.MaDVT = sp.MaDVT;
                    existing.GiaBan = sp.GiaBan;

                    db.SaveChanges();

                    return new ServiceResult(true, "Cập nhật sản phẩm thành công!");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Lỗi cập nhật: " + ex.Message);
            }
        }

        // 4. DELETE (Soft Delete)
        public ServiceResult Delete(string maSP)
        {
            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    // Bỏ điều kiện '&& x.IsDeleted == false' để tìm được cả sản phẩm đã ngừng
                    var sp = db.SAN_PHAM.FirstOrDefault(x => x.MaSP == maSP);

                    if (sp == null)
                        return new ServiceResult(false, "Không tìm thấy sản phẩm!");

                    // Chuyển trạng thái sang ngừng kinh doanh
                    sp.IsDeleted = true;

                    db.SaveChanges();
                    return new ServiceResult(true, "Đã chuyển sản phẩm sang trạng thái ngừng kinh doanh!");
                }
            }
            catch (Exception ex)
            {
                return new ServiceResult(false, "Lỗi: " + ex.Message);
            }
        }
    }
}
