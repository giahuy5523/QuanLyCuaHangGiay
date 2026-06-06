using QuanLyShopGiay.Models;
using QuanLyShopGiay.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace QuanLyShopGiay.Views.Pages
{
    public partial class HoaDonBanHangPage : Page
    {
        private HoaDonBanHangViewModel VM =>
            (HoaDonBanHangViewModel)DataContext;

        public HoaDonBanHangPage()
        {
            InitializeComponent();
        }

        // Search box — delegate xuống ViewModel
        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
        {
            VM.ApplyFilter(txtTimKiem.Text, VM.SelectedMaLoai);
        }

        // ComboBox filter loại — ViewModel tự xử lý qua SelectedMaLoai
        private void CboLocLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        // Click card sản phẩm → mở dialog chọn biến thể
        private void CardSanPham_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is SanPham sp)
                VM.ThemVaoGioCommand.Execute(sp);
        }

        // Xóa 1 item
        private void BtnXoaItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock tb &&
                tb.Tag is GioHangItem item)
                VM.XoaItemCommand.Execute(item);
        }

        // Xóa tất cả
        private void LblXoaTatCa_Click(object sender, MouseButtonEventArgs e)
        {
            var result = MessageBox.Show(
                "Xóa toàn bộ giỏ hàng?", "Xác nhận",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                VM.XoaTatCaCommand.Execute(null);
        }
    }
}