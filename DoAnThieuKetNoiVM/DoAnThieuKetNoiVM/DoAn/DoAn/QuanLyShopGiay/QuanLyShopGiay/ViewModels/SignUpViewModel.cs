using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class SignUpViewModel : BaseViewModel
    {
        // ════════════════════════════════════════════════════════════════════════
        // ACTION CALLBACKS
        // ════════════════════════════════════════════════════════════════════════
        public Action OnDangKyThanhCong { get; set; }
        public Action OnChuyenDangNhap { get; set; }

        // ════════════════════════════════════════════════════════════════════════
        // PROPERTIES
        // ════════════════════════════════════════════════════════════════════════

        private string _tenDangNhap = "";
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set { if (SetProperty(ref _tenDangNhap, value)) ClearLoi(); }
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set { if (SetProperty(ref _email, value)) ClearLoi(); }
        }

        private string _hoTen = "";
        public string HoTen
        {
            get => _hoTen;
            set { if (SetProperty(ref _hoTen, value)) ClearLoi(); }
        }

        private string _matKhau = "";
        public string MatKhau
        {
            get => _matKhau;
            set { if (SetProperty(ref _matKhau, value)) ClearLoi(); }
        }

        private string _xacNhanMatKhau = "";
        public string XacNhanMatKhau
        {
            get => _xacNhanMatKhau;
            set { if (SetProperty(ref _xacNhanMatKhau, value)) ClearLoi(); }
        }

        private string _maNV = "";
        public string MaNV
        {
            get => _maNV;
            set => SetProperty(ref _maNV, value);
        }

        private bool _isPasswordVisible = false;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        private bool _isConfirmVisible = false;
        public bool IsConfirmVisible
        {
            get => _isConfirmVisible;
            set => SetProperty(ref _isConfirmVisible, value);
        }

        private string _thongBaoLoi = "";
        public string ThongBaoLoi
        {
            get => _thongBaoLoi;
            set => SetProperty(ref _thongBaoLoi, value);
        }

        // ════════════════════════════════════════════════════════════════════════
        // COMMANDS
        // ════════════════════════════════════════════════════════════════════════

        public ICommand DangKyCommand { get; }
        public ICommand ChuyenDangNhapCommand { get; }

        public SignUpViewModel()
        {
            DangKyCommand = new RelayCommand(param => XuLyDangKy(param as Window));
            ChuyenDangNhapCommand = new RelayCommand(param => XuLyChuyenDangNhap(param as Window));
        }

        // ════════════════════════════════════════════════════════════════════════
        // COMMAND HANDLERS
        // ════════════════════════════════════════════════════════════════════════

        private void XuLyDangKy(Window window)
        {
            ThongBaoLoi = ""; // Reset lỗi trước khi validate lại
            string loi = ValidateInputs();
            if (!string.IsNullOrEmpty(loi))
            {
                ThongBaoLoi = loi;
                return;
            }

            try
            {
                using (var db = new QuanLyShopGiayEntities())
                {
                    // Kiểm tra tên đăng nhập đã tồn tại trong TAI_KHOAN chưa
                    if (db.TAI_KHOAN.Any(tk => tk.TenDangNhap == TenDangNhap.Trim()))
                    {
                        ThongBaoLoi = "Tên đăng nhập đã được sử dụng!";
                        return;
                    }

                    // Kiểm tra đã có yêu cầu đang chờ duyệt với tên này chưa
                    if (db.YEU_CAU_DANG_KY.Any(yc =>
                            yc.TenDangNhap == TenDangNhap.Trim() &&
                            yc.TrangThai == "ChoXet"))
                    {
                        ThongBaoLoi = "Tên đăng nhập này đang có yêu cầu chờ duyệt!";
                        return;
                    }

                    // Hash mật khẩu dùng đúng PasswordHelper như TaiKhoanService
                    var salt = Guid.NewGuid();
                    var matKhauHash = PasswordHelper.HashPassword(MatKhau, salt);

                    var yeuCau = new YEU_CAU_DANG_KY
                    {
                        TenDangNhap = TenDangNhap.Trim(),
                        Email = Email.Trim(),
                        HoTen = HoTen.Trim(),
                        MaNV = string.IsNullOrWhiteSpace(MaNV) ? null : MaNV.Trim(),
                        MatKhau = matKhauHash,
                        Salt = salt,
                        NgayTao = DateTime.Now,
                        TrangThai = "ChoXet"
                    };

                    db.YEU_CAU_DANG_KY.Add(yeuCau);
                    db.SaveChanges();
                }

                MessageBox.Show(
                    "Đăng ký thành công!\nTài khoản của bạn đang chờ Admin duyệt.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OnDangKyThanhCong?.Invoke();
                window?.Close();
            }
            catch (Exception ex)
            {
                ThongBaoLoi = "Đã xảy ra lỗi. Vui lòng thử lại.";
                System.Diagnostics.Debug.WriteLine("[SignUpViewModel] " + ex.Message);
            }
        }

        private void XuLyChuyenDangNhap(Window window)
        {
            OnChuyenDangNhap?.Invoke();
            window?.Close();
        }

        // ════════════════════════════════════════════════════════════════════════
        // VALIDATION
        // ════════════════════════════════════════════════════════════════════════

        private string ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TenDangNhap) ||
                string.IsNullOrWhiteSpace(HoTen) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(MatKhau))
                return "Vui lòng nhập đầy đủ: tên đăng nhập, họ tên, email và mật khẩu!";

            if (TenDangNhap.Trim().Length < 4)
                return "Tên đăng nhập phải có ít nhất 4 ký tự!";

            if (!Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                return "Email không đúng định dạng!";

            if (MatKhau.Length < 6)
                return "Mật khẩu phải có ít nhất 6 ký tự!";

            if (MatKhau != XacNhanMatKhau)
                return "Mật khẩu xác nhận không khớp!";

            return null;
        }

        private void ClearLoi()
        {
            if (!string.IsNullOrEmpty(ThongBaoLoi))
                ThongBaoLoi = "";
        }
    }
}