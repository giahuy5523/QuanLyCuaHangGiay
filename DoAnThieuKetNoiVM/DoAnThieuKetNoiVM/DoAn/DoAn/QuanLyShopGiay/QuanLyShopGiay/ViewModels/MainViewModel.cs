using QuanLyShopGiay.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using static QuanLyShopGiay.ViewModels.ucTimKiemViewModel;

namespace QuanLyShopGiay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _tieuDe = "Dashboard";

        // CÁC VIEWMODEL CON QUẢN LÝ HEADER TRÊN CÙNG
        private NhanVienHeaderViewModel _headerVM;
        private ucTimKiemViewModel _timKiemVM;

        public string HoTenNV => SessionManager.HoTenNV ?? SessionManager.TenDangNhap;
        public string TenVaiTro => SessionManager.TenVT ?? "N/A";
        public string NgayHienTai => DateTime.Now.ToString("dddd, dd/MM/yyyy");

        public string TieuDe
        {
            get => _tieuDe;
            set => SetProperty(ref _tieuDe, value);
        }

        public bool CoQuyenAdmin => SessionManager.IsAdmin;

        // Property quản lý thông tin nhân viên góc phải trên cùng
        public NhanVienHeaderViewModel HeaderVM
        {
            get => _headerVM;
            set => SetProperty(ref _headerVM, value);
        }

        // Property quản lý thanh tìm kiếm góc trái trên cùng
        public ucTimKiemViewModel TimKiemVM
        {
            get => _timKiemVM;
            set => SetProperty(ref _timKiemVM, value);
        }

        public ICommand NavDashboardCommand { get; }
        public ICommand NavSanPhamCommand { get; }
        public ICommand NavKhachHangCommand { get; }
        public ICommand NavHoaDonCommand { get; }
        public ICommand NavKhoCommand { get; }
        public ICommand NavNhanVienCommand { get; }
        public ICommand NavTaiKhoanCommand { get; }
        public ICommand DangXuatCommand { get; }

        public Action<string> Navigate { get; set; }
        public Action MoLoginView { get; set; }

        public MainViewModel()
        {
            // 1. Khởi tạo dữ liệu cho Header bằng thông tin đăng nhập từ Session
            HeaderVM = new NhanVienHeaderViewModel(HoTenNV, TenVaiTro);

            // 2. Khởi tạo ViewModel Tìm Kiếm và lắng nghe sự kiện tìm kiếm trực tiếp tại đây
            TimKiemVM = new ucTimKiemViewModel();
            TimKiemVM.XacNhanTimKiem += OnXacNhanTimKiem;

            // 3. Khởi tạo các lệnh điều hướng
            NavDashboardCommand = new RelayCommand(o => ChangeNav("Dashboard", "Dashboard"));
            NavSanPhamCommand = new RelayCommand(o => ChangeNav("SanPham", "Sản phẩm"));
            NavKhachHangCommand = new RelayCommand(o => ChangeNav("KhachHang", "Khách hàng"));
            NavHoaDonCommand = new RelayCommand(o => ChangeNav("HoaDon", "Hóa đơn"));
            NavKhoCommand = new RelayCommand(o => ChangeNav("KhoHang", "Kho hàng"));
            NavNhanVienCommand = new RelayCommand(o => ChangeNav("NhanVien", "Nhân viên"));
            NavTaiKhoanCommand = new RelayCommand(o => ChangeNav("TaiKhoan", "Tài khoản"));
            DangXuatCommand = new RelayCommand(o => ThucHienDangXuat());
        }

        private void ChangeNav(string page, string tieuDe)
        {
            TieuDe = tieuDe;
            Navigate?.Invoke(page);
        }

        /// <summary>
        /// Logic hứng từ khóa và danh mục từ thanh tìm kiếm truyền lên
        /// </summary>
        private void OnXacNhanTimKiem(object sender, SuKienTimKiemArgs e)
        {
            // Chỗ này bạn viết logic lọc dữ liệu thực tế tùy thuộc vào trang đang đứng nhé!
            // Ví dụ minh họa:
            // if (TieuDe == "Sản phẩm") { // Gọi hàm tìm sản phẩm... }
            MessageBox.Show($"[MainVM] Đang tìm kiếm cụm từ: '{e.TuKhoa}' trong danh mục: '{e.DanhMuc}'", "Kết quả bộ lọc");
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