using System;
using System.Windows;
using QuanLyShopGiay.Command;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyShopGiay.ViewModels
{
    public class ucTimKiemViewModel : BaseViewModel
    {
        public class SuKienTimKiemArgs : EventArgs
        {
            public string TuKhoa { get; set; }
            public string DanhMuc { get; set; }
        }
        public event EventHandler<SuKienTimKiemArgs> XacNhanTimKiem;

        // ĐÃ SỬA: Đổi tên _tuKhoa thành _searchKeyword để khớp với XAML
        private string _searchKeyword;
        private string _danhMucSelected = "Sản phẩm";

        // ĐÃ SỬA: Đổi tên thuộc tính thành SearchKeyword để Binding trong XAML nhận diện được
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(); }
        }

        public string DanhMucSelected
        {
            get => _danhMucSelected;
            set { _danhMucSelected = value; OnPropertyChanged(); }
        }

        // ĐÃ SỬA: Đổi tên thành SearchCommand để khớp với XAML
        public ICommand SearchCommand { get; }

        public ucTimKiemViewModel()
        {
            // Gán lệnh vào đúng tên command mới
            SearchCommand = new RelayCommand(ThucHienTimKiem);
        }

        private void ThucHienTimKiem(object parameter)
        {
            string tuKhoaXuLy = SearchKeyword?.Trim(); // Dùng SearchKeyword mới

            if (string.IsNullOrWhiteSpace(tuKhoaXuLy))
            {
                MessageBox.Show("Vui lòng nhập từ khóa trước khi tìm kiếm!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            XacNhanTimKiem?.Invoke(this, new SuKienTimKiemArgs
            {
                TuKhoa = tuKhoaXuLy,
                DanhMuc = DanhMucSelected ?? "Sản phẩm"
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}