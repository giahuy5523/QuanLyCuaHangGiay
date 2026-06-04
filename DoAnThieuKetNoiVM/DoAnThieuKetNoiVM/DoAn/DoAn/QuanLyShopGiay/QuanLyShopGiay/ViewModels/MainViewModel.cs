using QuanLyShopGiay.Helpers;
using System;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _tieuDe = "Dashboard";

        public string HoTenNV => SessionManager.HoTenNV ?? SessionManager.TenDangNhap;
        public string TenVaiTro => SessionManager.TenVT ?? "N/A";
        public string NgayHienTai => DateTime.Now.ToString("dddd, dd/MM/yyyy");

        public string TieuDe
        {
            get => _tieuDe;
            set => SetProperty(ref _tieuDe, value);
        }

        public bool CoQuyenAdmin => SessionManager.IsAdmin;

        public ICommand NavDashboardCommand { get; }
        public ICommand NavSanPhamCommand { get; }
        public ICommand NavKhachHangCommand { get; } // FIX: Thêm command còn thiếu
        public ICommand NavHoaDonCommand { get; }
        public ICommand NavKhoCommand { get; }
        public ICommand NavNhanVienCommand { get; }
        public ICommand NavTaiKhoanCommand { get; }
        public ICommand DangXuatCommand { get; }

        public Action<string> Navigate { get; set; }
        public Action MoLoginView { get; set; }

        public MainViewModel()
        {
            NavDashboardCommand = new RelayCommand(_ => ChangeNav("Dashboard", "Dashboard"));
            NavSanPhamCommand = new RelayCommand(_ => ChangeNav("SanPham", "Sản phẩm"));
            NavKhachHangCommand = new RelayCommand(_ => ChangeNav("KhachHang", "Khách hàng"));
            NavHoaDonCommand = new RelayCommand(_ => ChangeNav("HoaDon", "Hóa đơn"));
            NavKhoCommand = new RelayCommand(_ => ChangeNav("KhoHang", "Kho hàng"));
            NavNhanVienCommand = new RelayCommand(_ => ChangeNav("NhanVien", "Nhân viên"));
            NavTaiKhoanCommand = new RelayCommand(_ => ChangeNav("TaiKhoan", "Tài khoản"));
            DangXuatCommand = new RelayCommand(_ => ThucHienDangXuat());
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
                SessionManager.DangXuat();
                MoLoginView?.Invoke();
            }
        }
    }
}