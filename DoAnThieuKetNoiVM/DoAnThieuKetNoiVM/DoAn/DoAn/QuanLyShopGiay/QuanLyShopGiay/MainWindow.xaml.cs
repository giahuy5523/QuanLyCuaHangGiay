using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.ViewModels;
using QuanLyShopGiay.Views;
using QuanLyShopGiay.Views.Pages;
using System;
using System.Windows;

namespace QuanLyShopGiay
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            // 1. Kiểm tra đã đăng nhập chưa (dùng SessionManager)
            if (!SessionManager.DaDangNhap)
            {
                MessageBox.Show("Chưa đăng nhập!", "Lỗi");
                this.Close();
                return;
            }

            // 2. Khởi tạo ViewModel
            _vm = new MainViewModel();

            // 3. Gán DataContext TRƯỚC khi gán callback
            this.DataContext = _vm;

            // 4. Khởi tạo component (XAML)
            InitializeComponent();

            // 5. Định nghĩa callback điều hướng trang
            _vm.Navigate = (pageName) =>
            {
                try
                {
                    switch (pageName)
                    {
                        case "Dashboard":
                            MainFrame.Navigate(new DashboardPage());
                            break;
                        case "SanPham":
                            MainFrame.Navigate(new SanPhamPage());
                            break;
                        case "KhachHang":
                            MainFrame.Navigate(new KhachHangPage());
                            break;
                        case "HoaDon":
                            MainFrame.Navigate(new HoaDonBanHangPage());
                            break;
                        case "NhanVien":
                            MainFrame.Navigate(new NhanVienPage());
                            break;
                        case "TaiKhoan":
                            MainFrame.Navigate(new TaiKhoanPage());
                            break;
                        default:
                            MessageBox.Show($"Trang '{pageName}' không tồn tại!", "Cảnh báo");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi mở trang: {ex.Message}", "Lỗi");
                }
            };

            // 6. Định nghĩa callback đăng xuất
            _vm.MoLoginView = () =>
            {
                try
                {
                    // Đăng xuất session
                    SessionManager.DangXuat();

                    // Mở Login Window
                    var login = new Login();
                    login.Show();

                    // Đóng MainWindow
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi đăng xuất: {ex.Message}", "Lỗi");
                }
            };

            // 7. Hiển thị trang Dashboard mặc định
            MainFrame.Navigate(new DashboardPage());

            // 8. Cập nhật title (dùng SessionManager)
            Title = $"Quản Lý Shop Giày - {SessionManager.HoTenNV} ({SessionManager.TenVT})";
        }
    }
}