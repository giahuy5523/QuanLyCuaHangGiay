// Helpers/InverseBoolToVisibilityConverter.cs
// Converter đảo ngược BooleanToVisibilityConverter:
//   True  → Collapsed
//   False → Visible
//
// Dùng để hiện PasswordBox khi IsPasswordVisible = False,
// và ẩn PasswordBox khi IsPasswordVisible = True (lúc này TextBox được hiện).

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuanLyShopGiay.Helpers
{
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? Visibility.Collapsed : Visibility.Visible;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v != Visibility.Visible;
            return false;
        }
    }
}
