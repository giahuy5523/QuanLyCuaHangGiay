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
                if (string.IsNullOrWhiteSpace(TenDangNhap) || string.IsNullOrWhiteSpace(MatKhau))
                {
                    MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Thông báo");
                    return;
                }

                var user = db.NhanViens.FirstOrDefault(x =>
                          x.TenDangNhap == TenDangNhap &&
                          x.MatKhau == MatKhau);

                if (user != null)
                {
                    UserSession.MaNV = user.MaNhanVien;
                    UserSession.TenNV = user.TenNhanVien;
                    UserSession.Quyen = user.Quyen;

                    var loginWindow = Application.Current.Windows.OfType<Login>().FirstOrDefault();
                    if (loginWindow != null)
                    {
                        loginWindow.DialogResult = true;
                    }
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