using QuanLyShopGiay.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace QuanLyShopGiay.Views.Dialogs
{
    public partial class ChonBienTheDialog : Window
    {
        public SanPham SpDaChon { get; private set; }

        // Nhận vào list các SanPham cùng tên, khác size/màu
        public ChonBienTheDialog(List<SanPham> cacBienThe)
        {
            InitializeComponent();
            // Lấy tên SP chung (TenSP của item đầu)
            lblTenSP.Text = cacBienThe.FirstOrDefault()?.TenSP ?? "";
            lbBienThe.ItemsSource = cacBienThe;
        }

        private void BtnXacNhan_Click(object sender, RoutedEventArgs e)
        {
            if (lbBienThe.SelectedItem is SanPham sp)
            {
                SpDaChon = sp;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một biến thể!",
                    "Chưa chọn", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnHuy_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}