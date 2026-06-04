using System;
using System.Globalization;
using System.Windows.Data;

namespace QuanLyShopGiay.Helpers
{
    /// <summary>
    /// Converter: true → 1.0 (bình thường), false → 0.4 (mờ).
    /// Dùng cho nút sidebar bị disable do phân quyền.
    /// </summary>
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? 1.0 : 0.4;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
