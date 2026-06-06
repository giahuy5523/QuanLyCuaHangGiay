using QuanLyShopGiay.Command;
using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Views;
using System;
using System.Linq;
using System.Windows;

namespace QuanLyShopGiay.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _tenDangNhap;

        // ĐÃ SỬA: Đổi lại thành TenDangNhap để khớp Binding với ô Textbox ở giao diện Login.xaml
        public string TenDangNhap
        {
            get => _tenDangNhap;
            set
            {
                _tenDangNhap = value;
                OnPropertyChanged();
            }
        }

        private string _matKhau;

        public string MatKhau
        {
            get => _matKhau;
            set
            {
                _matKhau = value;
                OnPropertyChanged();
            }
        }

        QLShopGiayEntities3 db = new QLShopGiayEntities3();

        public RelayCommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(o =>
            {
                // Kiểm tra input đầu vào
                if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
                {
                    MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Thông báo");
                    return;
                }

                // ĐÃ SỬA: So sánh TenDangNhap và MatKhau, kèm bọc .Trim() để gọt sạch khoảng trắng thừa trong SQL (nếu có)
                var user = db.NhanViens.FirstOrDefault(x =>
                    x.TenDangNhap.Trim() == TenDangNhap.Trim() &&
                    x.MatKhau.Trim() == MatKhau.Trim());

                if (user != null)
                {
                    SessionManager.CurrentUser = user;

                    // Tìm cửa sổ Login đang hiển thị để chuyển màn hình
                    var loginWindow = Application.Current.Windows.OfType<Login>().FirstOrDefault();
                    if (loginWindow != null)
                    {
                        // 1. Mở màn hình chính MainWindow lên trước
                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();

                        // 2. Đóng hẳn form Đăng nhập lại an toàn
                        loginWindow.Close();
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu không đúng!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            });
        }
    }
}