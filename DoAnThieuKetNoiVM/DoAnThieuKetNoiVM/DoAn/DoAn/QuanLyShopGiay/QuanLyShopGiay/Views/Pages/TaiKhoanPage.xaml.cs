using QuanLyShopGiay.ViewModels;
using System.Windows.Controls;

namespace QuanLyShopGiay.Views.Pages
{
    public partial class TaiKhoanPage : Page
    {
        public TaiKhoanPage()
        {
            InitializeComponent();

            var viewModel = new TaiKhoanViewModel();
            this.DataContext = viewModel;

            // Wire-up PasswordBox vì PasswordBox không hỗ trợ binding trực tiếp
            viewModel.LayMatKhauMoi = () => pbMatKhauMoi.Password;
        }
    }
}