namespace QuanLyShopGiay.Helpers
{
    /// <summary>
    /// Lưu thông tin tài khoản đang đăng nhập, dùng được ở mọi nơi trong app.
    /// Gọi SessionManager.DangNhap(...) sau khi verify thành công.
    /// Gọi SessionManager.DangXuat() khi đăng xuất.
    /// </summary>
    public static class SessionManager
    {
        // Mã tài khoản đang đăng nhập, VD: "TK03"
        public static string MaTK { get; private set; }

        // Tên đăng nhập, VD: "sale1"
        public static string TenDangNhap { get; private set; }

        // Mã vai trò, VD: "VT01" / "VT02" / "VT03" / "VT04"
        public static string MaVT { get; private set; }

        // Tên vai trò đầy đủ, VD: "Admin" / "Bán hàng"
        public static string TenVT { get; private set; }

        // Họ tên nhân viên liên kết (có thể null nếu TK không gắn NV)
        public static string HoTenNV { get; private set; }

        // Kiểm tra nhanh quyền
        public static bool IsAdmin => MaVT == "VT01";
        public static bool IsKeToan => MaVT == "VT02";
        public static bool IsBanHang => MaVT == "VT03";
        public static bool IsThuKho => MaVT == "VT04";

        /// <summary>Gọi sau khi xác thực mật khẩu thành công</summary>
        public static void DangNhap(string maTK, string tenDangNhap,
                                     string maVT, string tenVT,
                                     string hoTenNV = null)
        {
            MaTK = maTK;
            TenDangNhap = tenDangNhap;
            MaVT = maVT;
            TenVT = tenVT;
            HoTenNV = hoTenNV;
        }

        /// <summary>Gọi khi đăng xuất</summary>
        public static void DangXuat()
        {
            MaTK = null;
            TenDangNhap = null;
            MaVT = null;
            TenVT = null;
            HoTenNV = null;
        }

        /// <summary>True nếu đang có ai đó đăng nhập</summary>
        public static bool DaDangNhap => !string.IsNullOrEmpty(MaTK);
    }
}