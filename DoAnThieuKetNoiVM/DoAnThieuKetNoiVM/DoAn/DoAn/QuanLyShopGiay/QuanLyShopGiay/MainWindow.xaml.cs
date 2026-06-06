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
            InitializeComponent();

            _vm = new MainViewModel();

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
                    // FIX: Thêm case KhachHang còn thiếu
                    case "KhachHang":
                        MainFrame.Navigate(new Views.Pages.KhachHangPage());
                        break;
                    case "HoaDon":
                        MainFrame.Navigate(new Views.Pages.HoaDonBanHangPage());
                        break;
                    case "NhapHang":
                        MainFrame.Navigate(new Views.Pages.DonNhapHangPage());
                        break;
                    case "NhanVien":
                        MainFrame.Navigate(new Views.Pages.NhanVienPage());
                        break;
                    case "TaiKhoan":
                        MainFrame.Navigate(new Views.Pages.TaiKhoanPage());
                        break;
                }
            };

            _vm.MoLoginView = () =>
            {
                var login = new Views.Login();
                login.WindowState = this.WindowState;
                login.Show();
                this.Close();
            };

            DataContext = _vm;

            MainFrame.Navigate(new Views.Pages.DashboardPage());
        }
    }
}