using System;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyShopGiay.ViewModels // Đã sửa thành ViewModels (có s) cho đồng bộ với toàn dự án
{
    public class NhanVienHeaderViewModel : BaseViewModel
    {
        private string _tenNhanVien;
        private string _quyenNhanVien; // Sửa lỗi chính tả nhỏ từ _quuyenNhanVien thành _quyenNhanVien

        // Thuộc tính hiển thị Tên nhân viên
        public string TenNhanVien
        {
            get => _tenNhanVien;
            set
            {
                _tenNhanVien = value;
                // Sử dụng thẳng hàm của lớp cha BaseViewModel
                OnPropertyChanged();
            }
        }

        // Thuộc tính hiển thị Quyền (Vai trò)
        public string QuyenNhanVien
        {
            get => _quyenNhanVien;
            set
            {
                _quyenNhanVien = value;
                // Sử dụng thẳng hàm của lớp cha BaseViewModel
                OnPropertyChanged();
            }
        }

        // Khởi tạo không tham số (Mặc định)
        public NhanVienHeaderViewModel() { }

        // Hàm khởi tạo nhanh có tham số
        public NhanVienHeaderViewModel(string tenDN, string quyen)
        {
            TenNhanVien = tenDN;
            QuyenNhanVien = quyen;
        }

   
    }
}