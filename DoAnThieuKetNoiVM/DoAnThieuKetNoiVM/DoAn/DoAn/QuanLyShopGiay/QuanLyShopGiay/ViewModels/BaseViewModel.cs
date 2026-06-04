using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace QuanLyShopGiay.ViewModels
{
    /// <summary>
    /// Lớp nền cho tất cả ViewModel.
    /// Cài đặt INotifyPropertyChanged để binding tự động cập nhật UI.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gọi khi một property thay đổi giá trị → UI tự refresh.
        /// Dùng [CallerMemberName] để không cần truyền tên property thủ công.
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Set giá trị và tự động raise PropertyChanged nếu giá trị thay đổi.
        /// Dùng trong setter của property thay cho assignment thông thường.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}