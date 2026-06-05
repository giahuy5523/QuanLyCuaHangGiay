using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyShopGiay.ViewModels
{
    public class ucTimKiemViewModel : INotifyPropertyChanged
    {
        public class SuKienTimKiemArgs : EventArgs
        {
            public string TuKhoa { get; set; }
            public string DanhMuc { get; set; }
        }
        public event EventHandler<SuKienTimKiemArgs> XacNhanTimKiem;

        private string _tuKhoa;
        private string _danhMucSelected = "Sản phẩm";

        public string TuKhoa
        {
            get => _tuKhoa;
            set { _tuKhoa = value; OnPropertyChanged(); }
        }

        public string DanhMucSelected
        {
            get => _danhMucSelected;
            set { _danhMucSelected = value; OnPropertyChanged(); }
        }

        public ICommand TimKiemCommand { get; }

        public ucTimKiemViewModel()
        {
            // Nó sẽ tự động dùng RelayCommand có sẵn trong project của bạn
            TimKiemCommand = new RelayCommand(ThucHienTimKiem);
        }

        private void ThucHienTimKiem(object parameter)
        {
            string tuKhoaXuLy = TuKhoa?.Trim();

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

