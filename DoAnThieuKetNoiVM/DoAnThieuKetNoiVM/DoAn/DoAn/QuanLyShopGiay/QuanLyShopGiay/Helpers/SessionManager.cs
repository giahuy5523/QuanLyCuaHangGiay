using QuanLyShopGiay.Models;

namespace QuanLyShopGiay.Helpers
{
    /// <summary>
    /// Lưu thông tin tài khoản đang đăng nhập, dùng được ở mọi nơi trong app.
    /// Gọi SessionManager.DangNhap(...) sau khi verify thành công.
    /// Gọi SessionManager.DangXuat() khi đăng xuất.
    /// </summary>
    public static class SessionManager
    {
        public static NhanVien CurrentUser { get; set; }
        public static string MaNhanVienHienTai => UserSession.MaNV;
        // Kiểm tra xem người dùng hiện tại có phải là Quản lý/Admin hay không
        public static bool IsAdmin
        {
            get
            {
                if (CurrentUser == null) return false;
                return CurrentUser.Quyen == "QuanLy";
            }
        }

        // Kiểm tra xem người dùng có phải là Nhân viên bán hàng không
        public static bool IsBanHang => CurrentUser?.Quyen == "BanHang";

        // Kiểm tra xem người dùng có phải là Nhân viên kho không
        public static bool IsKhoQuy => CurrentUser?.Quyen == "KhoQuy";

        // Hàm xóa phiên đăng nhập khi Đăng xuất
        public static void ClearSession()
        {
            CurrentUser = null;
        }
    }
}