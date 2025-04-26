using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;

namespace FinanceTracker.Converters
{
    public class EnumToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            // Get the type of the enum
            Type enumType = value.GetType();
            
            // Check if it's an enum
            if (!enumType.IsEnum)
                return null;

            // Get all values of the enum
            var enumValues = Enum.GetValues(enumType);
            
            // Convert to a list of strings
            var result = new List<object>();
            foreach (var enumValue in enumValues)
            {
                result.Add(enumValue);
            }
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
