using QuanLyShopGiay.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyShopGiay.ViewModel
{
    public class NhanVienHeaderViewModel : BaseViewModel
    {
        private string _tenNhanVien;
        private string _quuyenNhanVien;

        // Thuộc tính hiển thị Tên nhân viên
        public string TenNhanVien
        {
            get => _tenNhanVien;
            set
            {
                _tenNhanVien = value;
                OnPropertyChanged();
            }
        }

        // Thuộc tính hiển thị Quyền (Vai trò)
        public string QuyenNhanVien
        {
            get => _quuyenNhanVien;
            set
            {
                _quuyenNhanVien = value;
                OnPropertyChanged();
            }
        }

        // Khởi tạo không tham số (Mặc định)
        public NhanVienHeaderViewModel() { }

        // Hàm khởi tạo nhanh có tham số (tương đương logic NapThongTin cũ)
        public NhanVienHeaderViewModel(string tenDN, string quyen)
        {
            TenNhanVien = tenDN;
            QuyenNhanVien = quyen;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}