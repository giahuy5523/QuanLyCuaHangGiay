using QuanLyShopGiay.Models;
using QuanLyShopGiay.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyShopGiay.Views.Pages
{
    public partial class NhanVienPage : Page
    {
        
        public NhanVienPage()
        {
            InitializeComponent();
            if (this.DataContext is NhanVienViewModel vm)
            {
                // Gọi hàm xử lý quyền thủ công
                vm.LoadQuyenCommand.Execute(null);
            }
        }
 
    }
}