using System.Windows;

namespace QuanLyShopGiay.Views
{
    public partial class Login: Window
    {
        public Login()
        {
            InitializeComponent();

            // Focus vào textbox tên đăng nhập khi load
            this.Loaded += (s, e) => TxtTenDangNhap.Focus();
        }
    }
}