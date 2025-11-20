using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogicTool.UI.Converters
{
    /// <summary>
    /// Конвертер строк в Visibility (Visible, если строка непуста).
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasText = value is string str && !string.IsNullOrWhiteSpace(str);
            return hasText ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

