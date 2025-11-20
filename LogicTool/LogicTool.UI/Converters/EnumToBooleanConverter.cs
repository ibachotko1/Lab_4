using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LogicTool.UI.Converters
{
    /// <summary>
    /// Конвертер, сопоставляющий перечисление булевому значению для биндинга RadioButton.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return DependencyProperty.UnsetValue;
            }

            string enumString = parameter.ToString();
            var enumValue = Enum.Parse(value.GetType(), enumString);
            return enumValue.Equals(value);
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null)
            {
                return Binding.DoNothing;
            }

            if (!(value is bool) || !(bool)value)
            {
                return Binding.DoNothing;
            }

            return Enum.Parse(targetType, parameter.ToString());
        }
    }
}

