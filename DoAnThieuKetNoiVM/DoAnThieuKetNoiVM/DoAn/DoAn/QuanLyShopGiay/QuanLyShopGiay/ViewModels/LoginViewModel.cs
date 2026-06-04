using QuanLyShopGiay.Models;
using QuanLyShopGiay.Views;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace QuanLyShopGiay.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private byte[] ToMD5ByteArray(string plaintext)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            }
        }
        private string _maNV;
        public string MaNV
        {
            get => _maNV;
            set { _maNV = value; OnPropertyChanged(); }
        }

        private string _matKhau;
        public string MatKhau
        {
            get => _matKhau;
            set { _matKhau = value; OnPropertyChanged(); }
        }

        private QLShopGiayEntities3 db = new QLShopGiayEntities3();

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(o=>
            {
                // 1. Kiểm tra đầu vào
                if (string.IsNullOrWhiteSpace(MaNV) || string.IsNullOrWhiteSpace(MatKhau))
                {
                    MessageBox.Show("Tên đăng nhập và mật khẩu không được để trống!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Mã hóa mật khẩu người dùng nhập sang dạng mảng byte đã băm MD5
                byte[] hashedMatKhau = ToMD5ByteArray(MatKhau);

                // 2. Tìm kiếm nhân viên trong Database
                var user = db.NhanVien.AsEnumerable().FirstOrDefault(x =>
                    x.MaNV.Trim() == MaNV.Trim() &&
                    x.MatKhau.SequenceEqual(hashedMatKhau)); 
                if (user != null)
                {
                    // SỬA LỖI CS0103 (UserSession): Gán trực tiếp vào thuộc tính tĩnh của App 
                    // hoặc lưu tạm vào Properties của Application để không bị lỗi thiếu Class
                    Application.Current.Properties["MaNV"] = user.MaNV;
                    Application.Current.Properties["TenNV"] = user.HoTen;

                    // 3. Mở giao diện chính MainWindow
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();

                    // 4. Đóng giao diện Đăng nhập hiện tại
                    // SỬA LỖI CS0246: Tìm đúng cửa sổ đang kích hoạt (ActiveWindow) để đóng, không cần chỉ định chính xác tên class Login_View
                    var loginWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive || w.DataContext == this);
                    loginWindow?.Close();
                }
                else
                {
                    MessageBox.Show(
                        "Tên đăng nhập hoặc mật khẩu không đúng!",
                        "Thông báo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });
        }
    }
}