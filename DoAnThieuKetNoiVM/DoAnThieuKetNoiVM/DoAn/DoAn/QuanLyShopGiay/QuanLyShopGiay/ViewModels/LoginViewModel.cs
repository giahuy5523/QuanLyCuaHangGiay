using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Views;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _tenDangNhap;
        private string _matKhau;
        private string _thongBaoLoi;
        private bool _dangXuLy;

        public string TenDangNhap
        {
            get => _tenDangNhap;
            set => SetProperty(ref _tenDangNhap, value);
        }

        public string MatKhau
        {
            get => _matKhau;
            set => SetProperty(ref _matKhau, value);
        }

        public string ThongBaoLoi
        {
            get => _thongBaoLoi;
            set => SetProperty(ref _thongBaoLoi, value);
        }

        public bool DangXuLy
        {
            get => _dangXuLy;
            set => SetProperty(ref _dangXuLy, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand ChuyenDangKyCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(p => ThucHienDangNhap(p as Window), _ => !DangXuLy);

            // SỬA Ở ĐÂY: Truyền tham số Window vào hàm ThucHienChuyenDangKy
            ChuyenDangKyCommand = new RelayCommand(p => ThucHienChuyenDangKy(p as Window));
        }
        private void ThucHienChuyenDangKy(Window currentWindow)
        {
            SignUp signUpWindow = new SignUp();
            signUpWindow.Show();
            currentWindow?.Close(); // Đóng màn hình Login
        }
        private void ThucHienDangNhap(Window currentWindow)
        {
            ThongBaoLoi = string.Empty;

            string tenDN = TenDangNhap?.Trim();
            string matKhau = MatKhau?.Trim(); // Lấy trực tiếp từ thuộc tính đã được Bind dữ liệu liên tục

            if (string.IsNullOrWhiteSpace(tenDN) || string.IsNullOrWhiteSpace(matKhau))
            {
                ThongBaoLoi = "Vui lòng nhập tên đăng nhập và mật khẩu!";
                return;
            }

            DangXuLy = true;
            try
            {
                TAI_KHOAN taiKhoan = null;

                using (var db = new QLShopGiayEntities())
                {
                    taiKhoan = db.TAI_KHOAN
                        .Include("VAI_TRO")
                        .Include("NHAN_VIEN")
                        .FirstOrDefault(tk => tk.TenDangNhap == tenDN && tk.IsDeleted != true);
                }

                if (taiKhoan == null)
                {
                    ThongBaoLoi = "Tên đăng nhập không tồn tại hoặc tài khoản đã bị khoá!";
                    return;
                }

                if (taiKhoan.Salt == null || taiKhoan.MatKhau == null)
                {
                    ThongBaoLoi = "Tài khoản chưa có mật khẩu. Liên hệ Admin!";
                    return;
                }

                byte[] hashNhap = PasswordHelper.HashPassword(matKhau, taiKhoan.Salt.Value);
                if (!hashNhap.SequenceEqual(taiKhoan.MatKhau))
                {
                    ThongBaoLoi = "Mật khẩu không đúng!";
                    return;
                }

                if (taiKhoan.NHAN_VIEN?.IsDeleted == true)
                {
                    ThongBaoLoi = "Tài khoản bị vô hiệu hóa (nhân viên đã nghỉ việc)!";
                    return;
                }

                // Lưu dữ liệu đăng nhập vào Session toàn cục
                SessionManager.DangNhap(
                    maTK: taiKhoan.MaTK,
                    tenDangNhap: taiKhoan.TenDangNhap,
                    maVT: taiKhoan.MaVT,
                    tenVT: taiKhoan.VAI_TRO?.TenVT,
                    hoTenNV: taiKhoan.NHAN_VIEN?.HoTen
                );

                // Mở màn hình ứng dụng chính MainWindow
                MainWindow main = new MainWindow();
                main.Show();

                // Tự động đóng màn hình Login hiện tại thông qua tham số giao diện truyền xuống
                currentWindow?.Close();
            }
            finally
            {
                DangXuLy = false;
            }
        }
    }
}