using System;
using System.Linq;
using QuanLyShopGiay.Models;

namespace QuanLyShopGiay.Utilities
{
    public static class InitializePasswordHelper
    {
        /// <summary>
        /// Khởi tạo test password cho các nhân viên
        /// Chỉ chạy lần đầu, sau đó comment lại ở App.xaml.cs
        /// </summary>
        public static void SetTestPasswords()
        {
            try
            {
                var db = new  QLShopGiayEntities();

                var testAccounts = new[]
                {
                    new { MaNV = "NV01", MatKhauPlain = "admin123", TenNhanVien = "Nguyễn Quản Lý" },
                    new { MaNV = "NV02", MatKhauPlain = "cashier123", TenNhanVien = "Trần Bán Hàng" },
                    new { MaNV = "NV03", MatKhauPlain = "cashier123", TenNhanVien = "Lê Thu Ngân" },
                    new { MaNV = "NV04", MatKhauPlain = "stock123", TenNhanVien = "Phạm Kho Quỹ" }
                };

                bool hasChanges = false;

                foreach (var account in testAccounts)
                {
                    var nv = db.NhanViens.FirstOrDefault(n => n.MaNhanVien == account.MaNV);
                    if (nv != null)
                    {
                        // Nếu chưa có mật khẩu, thì tạo mới (hoặc cập nhật nếu cần)
                        if (string.IsNullOrEmpty(nv.MatKhau) || nv.MatKhau != account.MatKhauPlain)
                        {
                            nv.MatKhau = account.MatKhauPlain; // Tạm thời lưu plaintext, sau sẽ hash
                            hasChanges = true;
                            System.Diagnostics.Debug.WriteLine($"Đã cập nhật mật khẩu cho {account.TenNhanVien}");
                        }
                    }
                }

                if (hasChanges)
                {
                    db.SaveChanges();
                    System.Diagnostics.Debug.WriteLine("Test passwords initialized!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo password: {ex.Message}");
                throw;
            }
        }
    }
}