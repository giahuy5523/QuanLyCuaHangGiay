using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyShopGiay.Models
{
    public class GioHangItemModel : INotifyPropertyChanged
    {
        public int MaBienThe { get; set; }
        public string TenSP { get; set; }

        // Hiển thị dạng: "Size: 40 - Màu: Trắng"
        public string TenBienThe { get; set; }

        public decimal DonGia { get; set; }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                if (_soLuong != value)
                {
                    _soLuong = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ThanhTien)); // Báo cho UI cập nhật lại Thành Tiền
                }
            }
        }

        // Tự động tính toán tiền của món hàng đó
        public decimal ThanhTien => DonGia * SoLuong;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
