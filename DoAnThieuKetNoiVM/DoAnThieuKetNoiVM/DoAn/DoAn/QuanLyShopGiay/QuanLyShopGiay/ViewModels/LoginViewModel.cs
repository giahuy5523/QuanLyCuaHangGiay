using  QuanLyShopGiay.Command;
using  QuanLyShopGiay.Helpers;
using  QuanLyShopGiay.Models;
using  QuanLyShopGiay.Views;
using QuanLyShopGiay.ViewModels;
using System.Linq;
using System.Windows;
using System.Security.Cryptography; 
using System.Text;


namespace  QuanLyShopGiay.ViewModels
{
    class LoginModelView : BaseViewModel
    {
        private string _tenDangNhap;

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

        public LoginModelView()
        {
            LoginCommand = new RelayCommand(o =>
            {
                // Kiểm tra input
                if (string.IsNullOrWhiteSpace(TenDangNhap) ||string.IsNullOrWhiteSpace(MatKhau))
                {
                    MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Thông báo");
                    return;
                }

                // 1. Mã hóa mật khẩu người dùng nhập sang dạng mảng byte đã băm MD5
                var user = db.NhanViens.FirstOrDefault(x =>
                          x.TenDangNhap == TenDangNhap &&
                          x.MatKhau == MatKhau);

                if (user != null)
                {
                    // Lưu session
                    UserSession.MaNV = user.MaNhanVien;
                    UserSession.TenNV = user.TenNhanVien;

                    // Mở MainWindow
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    // Đóng Login Window
                    Application.Current.Windows
                        .OfType<Window>()
                        .SingleOrDefault(x => x is Login)
                        ?.Close();
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