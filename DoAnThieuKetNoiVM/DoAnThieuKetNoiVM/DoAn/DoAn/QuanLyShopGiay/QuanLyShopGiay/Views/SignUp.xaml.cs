// SignUp.xaml.cs
// Đã sửa:
// [1] Bỏ toàn bộ logic toggle mật khẩu thủ công (chkHienMatKhau_Checked/Unchecked).
//     Việc toggle giờ do ViewModel quản lý qua IsPasswordVisible / IsConfirmVisible.
// [2] Code-behind chỉ còn InitializeComponent() — đúng chuẩn MVVM.
// [3] DataContext vẫn set trong XAML, không cần set lại ở đây.

using System.Windows;

namespace QuanLyShopGiay.Views
{
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }
    }
}
