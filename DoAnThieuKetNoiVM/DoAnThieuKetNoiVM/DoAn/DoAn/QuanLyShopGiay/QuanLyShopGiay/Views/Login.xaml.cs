using System.Windows;

namespace QuanLyShopGiay.Views
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void chkHienMatKhau_Checked(object sender, RoutedEventArgs e)
        {
            // Chép mật khẩu từ PasswordBox sang TextBox rồi hiện TextBox
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
            txtPasswordVisible.Focus();
        }

        private void chkHienMatKhau_Unchecked(object sender, RoutedEventArgs e)
        {
            // Chép ngược lại từ TextBox về PasswordBox rồi ẩn TextBox
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
            txtPassword.Focus();
        }
    }
}