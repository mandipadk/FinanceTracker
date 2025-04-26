using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FinanceTracker.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            // Get the string value of the enum
            string enumValue = value.ToString();
            string targetValue = parameter.ToString();

            // Compare the enum value with the target value
            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;

            bool boolValue = (bool)value;
            
            // If the boolean value is true, return the enum value
            if (boolValue)
                return Enum.Parse(targetType, parameter.ToString());

            // Return default value of the enum
            return Activator.CreateInstance(targetType);
        }
    }
}
