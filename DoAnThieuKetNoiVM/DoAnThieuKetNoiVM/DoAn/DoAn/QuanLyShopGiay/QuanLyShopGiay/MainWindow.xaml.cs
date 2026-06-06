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
            // 1. Khởi tạo UI component từ XAML trước
            InitializeComponent();

            // 2. Lấy DataContext đã khai báo ở XAML để không bị đơ nút bấm (Khởi tạo kép)
            _vm = this.DataContext as MainViewModel;
            if (_vm == null)
            {
                _vm = new MainViewModel();
                this.DataContext = _vm;
            }

            // ĐỒNG BỘ: Ép dữ liệu từ UserSession đăng nhập trực tiếp vào ViewModel để Sidebar nhìn thấy
            if (_vm != null)
            {
                _vm.TenNhanVien = UserSession.TenNV;
                _vm.Quyen = UserSession.Quyen;
            }

            // 3. Định nghĩa callback chuyển trang cho Frame
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

            // 4. Định nghĩa callback đăng xuất
            _vm.MoLoginView = () =>
            {
                try
                {
                    // Xóa dữ liệu phiên làm việc tĩnh
                    UserSession.Logout();

                    // Mở Login mới dạng thường
                    var login = new Login();
                    login.Show();

                    // Đóng MainWindow an toàn
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi đăng xuất: {ex.Message}", "Lỗi");
                }
            };

            // 5. Hiển thị trang Dashboard mặc định khi vừa vào app
            MainFrame.Navigate(new DashboardPage());

            // 6. Cập nhật title Window ăn theo dữ liệu tĩnh UserSession chuẩn chỉnh
            Title = $"Quản Lý Shop Giày - {UserSession.TenNV} ({UserSession.Quyen})";
        }
    }
}