using QuanLyShopGiay.Command;
using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using QuanLyShopGiay.Views.Pages;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _tieuDe = "Dashboard";
        private Frame _mainFrame;

        public string HoTenNV => !string.IsNullOrEmpty(UserSession.TenNV) ? UserSession.TenNV : "Chưa đăng nhập";
        public string TenVaiTro => !string.IsNullOrEmpty(UserSession.Quyen) ? UserSession.Quyen : "N/A";

        public string NgayHienTai => DateTime.Now.ToString("dddd, dd/MM/yyyy");

        public string TieuDe
        {
            get => _tieuDe;
            set
            {
                _tieuDe = value;
                OnPropertyChanged();
            }
        }
        public bool CoQuyenAdmin => UserSession.Quyen == "Admin" || UserSession.Quyen == "Quản lý";

        public ICommand NavDashboardCommand { get; }
        public ICommand NavSanPhamCommand { get; }
        public ICommand NavKhachHangCommand { get; }
        public ICommand NavHoaDonBanHangCommand { get; }
        public ICommand NavHoaDonNhapHangCommand { get; }
        public ICommand NavNhanVienCommand { get; }
        public ICommand NavTaiKhoanCommand { get; }
        public ICommand NavLoaiSanPhamCommand { get; }
        
        // THÊM: Lệnh điều hướng cho Nhà cung cấp
        public ICommand NavNhaCungCapCommand { get; }
        
        public ICommand DangXuatCommand { get; }
        public ICommand BaoCaoTonKhoCommand { get; }
        public ICommand XemThongKeMenuCommand { get; }

        public Action<string> Navigate { get; set; }
        public Action MoLoginView { get; set; }
        public void SetFrame(Frame frame)
        {
            _mainFrame = frame;
        }
        public MainViewModel()
        {
            NavDashboardCommand = new RelayCommand(_ => ChangeNav("Dashboard", "Dashboard"));
            NavSanPhamCommand = new RelayCommand(_ => ChangeNav("SanPham", "Sản phẩm"));
            NavKhachHangCommand = new RelayCommand(_ => ChangeNav("KhachHang", "Khách hàng"));
            NavHoaDonBanHangCommand = new RelayCommand(_ => ChangeNav("HoaDonBanHang", "Hóa đơn bán hàng"));
            NavHoaDonNhapHangCommand = new RelayCommand(_ => ChangeNav("NhapHang", "Hóa đơn nhập hàng"));
            NavLoaiSanPhamCommand = new RelayCommand(_ => ChangeNav("LoaiSanPham", "Loại sản phẩm"));
            NavNhanVienCommand = new RelayCommand(_ => ChangeNav("NhanVien", "Nhân viên"));
            NavTaiKhoanCommand = new RelayCommand(_ => ChangeNav("TaiKhoan", "Tài khoản"));

            BaoCaoTonKhoCommand = new RelayCommand(_ => MoTonKho());
            XemThongKeMenuCommand = new RelayCommand(_ => MoThongKe());
        }

        private void ChangeNav(string page, string tieuDe)
        {
            TieuDe = tieuDe;
            Navigate?.Invoke(page);
        }

        private void ThucHienDangXuat()
        {
            var result = MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                UserSession.Logout();
                MoLoginView?.Invoke();
            }
        }
        private void MoTonKho()
        {
            var vm = new InBaoCaoViewModel
            {
                LoaiBaoCao = "TonKho"
            };

            vm.KhoiTaoBaoCao();

            _mainFrame?.Navigate(new InBaoCaoPage(vm));

            TieuDe = "Báo Cáo Tồn Kho";
        }

        private void MoThongKe()
        {
            var vm = new InBaoCaoViewModel
            {
                LoaiBaoCao = "ThongKe",
                TuNgay = DateTime.Now.AddMonths(-1),
                DenNgay = DateTime.Now
            };
            vm.KhoiTaoBaoCao();

            _mainFrame?.Navigate(new InBaoCaoPage(vm));
            TieuDe = "Thống Kê Doanh Thu";
        }
    }
}
