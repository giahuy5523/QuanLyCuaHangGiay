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

            // ĐIỀU HƯỚNG TRANG
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
                    case "LoaiSanPham":
                        MainFrame.Navigate(new Views.Pages.LoaiSanPhamPage());
                        break;
                    case "KhachHang":
                        MainFrame.Navigate(new Views.Pages.KhachHangPage());
                        break;
                    case "HoaDonBanHang":
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

            // ĐĂNG XUẤT QUAY VỀ LOGIN
            _vm.MoLoginView = () =>
            {
                var login = new Views.Login();
                login.WindowState = this.WindowState;
                login.Show();
                this.Close();
            };

            // GÁN DATACONTEXT ĐỂ KÍCH HOẠT BINDING
            DataContext = _vm;

            

            // TRANG MẶC ĐỊNH KHI MỞ APP
            MainFrame.Navigate(new Views.Pages.DashboardPage());
        }
    }
}