using QuanLyShopGiay.Command;
using QuanLyShopGiay.Helpers;
using QuanLyShopGiay.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _tieuDe = "Dashboard";

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

        public Action<string> Navigate { get; set; }
        public Action MoLoginView { get; set; }

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
            
            // THÊM: Khởi tạo lệnh điều hướng tương ứng sang trang "NhaCungCap" với tiêu đề tương ứng
            NavNhaCungCapCommand = new RelayCommand(_ => ChangeNav("NhaCungCap", "Nhà cung cấp"));
            
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
                UserSession.Logout();
                MoLoginView?.Invoke();
            }
        }
    }
}
