using System;
using System.Globalization;
using FinanceTracker.Models;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace FinanceTracker.Converters
{
    public class TransactionTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionType type)
            {
                return type switch
                {
                    TransactionType.Income => Color.FromArgb("#4CAF50"), // Green for income
                    TransactionType.Expense => Color.FromArgb("#F44336"), // Red for expense
                    _ => Colors.Gray
                };
            }
            
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
