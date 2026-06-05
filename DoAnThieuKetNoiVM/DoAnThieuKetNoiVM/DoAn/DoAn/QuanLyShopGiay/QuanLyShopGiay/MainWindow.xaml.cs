using QuanLyShopGiay.ViewModels;
using System;
using System.Windows;

namespace QuanLyShopGiay
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm;

        public MainWindow()
        {
            InitializeComponent();
            // 2. Định nghĩa sự kiện điều hướng trang (Bắt buộc phải viết TRƯỚC khi gán DataContext)
            _vm.Navigate = (pageName) =>
            {
                switch (pageName)
                {
                    case "Dashboard":
                        MainFrame.Navigate(new Views.Pages.DashboardPage());
                        break;
                    case "SanPham":
                        MainFrame.Navigate(new Views.Pages.SanPhamPage());
                        break;
                    case "KhachHang":
                        MainFrame.Navigate(new Views.Pages.KhachHangPage());
                        break;
                    case "HoaDon":
                        MainFrame.Navigate(new Views.Pages.HoaDonBanHangPage());
                        break;
                    case "NhanVien":
                        MainFrame.Navigate(new Views.Pages.NhanVienPage());
                        break;
                    case "TaiKhoan":
                        MainFrame.Navigate(new Views.Pages.TaiKhoanPage());
                        break;
                }
            };

            // 3. Định nghĩa sự kiện mở màn hình đăng nhập khi Đăng xuất
            _vm.MoLoginView = () =>
            {
                var login = new Views.Login();
                login.WindowState = this.WindowState;
                login.Show();
                this.Close();
            };

            // 4. Gán DataContext để các Button ngoài XAML nhận được Command
            this.DataContext = _vm;

            // 5. Hiển thị trang Dashboard mặc định lúc vừa mở app
            MainFrame.Navigate(new Views.Pages.DashboardPage());
        }
    }
}