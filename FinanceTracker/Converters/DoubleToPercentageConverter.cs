using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace FinanceTracker.Converters
{
    public class DoubleToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // Convert from percentage (0-100) to a value between 0 and 1
                return doubleValue / 100.0;
            }
            
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // Convert from a value between 0 and 1 to percentage (0-100)
                return doubleValue * 100.0;
            }
            
            return 0;
        }
    }
}
