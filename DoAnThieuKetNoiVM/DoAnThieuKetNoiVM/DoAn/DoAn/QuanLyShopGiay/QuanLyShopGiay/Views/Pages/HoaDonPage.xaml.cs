//using QuanLyShopGiay.Helpers;
//using QuanLyShopGiay.Models;
//using QuanLyShopGiay.ViewModels;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.IO;
//using System.Linq;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using System.Windows.Media.Imaging;

//namespace QuanLyShopGiay.Views.Pages
//{
//    public partial class HoaDonPage : Page
//    {
//        private HoaDonViewModel _vm;

//        public HoaDonPage()
//        {
//            InitializeComponent();
//            _vm = new HoaDonViewModel();
//            DataContext = _vm; // Kích hoạt DataContext liên kết dữ liệu
//        }

//        // ─── Tìm kiếm sản phẩm khi gõ chữ ─────────────────────────────
//        private void TxtTimKiem_TextChanged(object sender, TextChangedEventArgs e)
//        {
//            string keyword = txtTimKiem.Text;
//            string maLoai = cboLocLoai.SelectedValue?.ToString() ?? "";
//            _vm.LocSanPham(keyword, maLoai);
//        }

//        // ─── Lọc theo loại hàng khi chọn ComboBox ──────────────────────
//        private void CboLocLoai_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            string keyword = txtTimKiem.Text;
//            string maLoai = cboLocLoai.SelectedValue?.ToString() ?? "";
//            _vm.LocSanPham(keyword, maLoai);
//        }

//        // ─── Click chọn sản phẩm đổ vào giỏ hàng ───────────────────────
//        private void CardSanPham_Click(object sender, MouseButtonEventArgs e)
//        {
//            // Ép kiểu đối tượng lấy từ Tag của Border sang SAN_PHAM_DTO (hoặc SAN_PHAM tùy cấu trúc ListSanPham của bạn)
//            if (sender is Border border && border.Tag != null)
//            {
//                _vm.ThemVaoGioCommand.Execute(border.Tag);
//            }
//        }

//        // ─── Xoá tất cả item trong giỏ hàng ────────────────────────────
//        private void LblXoaTatCa_Click(object sender, MouseButtonEventArgs e)
//        {
//            _vm.XoaTatCaCommand.Execute(null);
//        }

//        // ─── Xoá cụ thể 1 dòng hàng khỏi giỏ ───────────────────────────
//        private void BtnXoaItem_Click(object sender, MouseButtonEventArgs e)
//        {
//            if (sender is TextBlock txt && txt.Tag is GioHangItem item)
//            {
//                _vm.XoaItemCommand.Execute(item);
//            }
//        }
//    }

//    }

