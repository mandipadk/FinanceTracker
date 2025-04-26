using System;
using System.Globalization;
using FinanceTracker.Models;
using Microsoft.Maui.Controls;

namespace FinanceTracker.Converters
{
    public class CategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionCategory category)
            {
                return category switch
                {
                    TransactionCategory.Salary => "💼",
                    TransactionCategory.Investment => "📈",
                    TransactionCategory.Gift => "🎁",
                    TransactionCategory.Food => "🍔",
                    TransactionCategory.Housing => "🏠",
                    TransactionCategory.Transportation => "🚗",
                    TransactionCategory.Entertainment => "🎬",
                    TransactionCategory.Shopping => "🛍️",
                    TransactionCategory.Utilities => "💡",
                    TransactionCategory.Healthcare => "🏥",
                    TransactionCategory.Education => "🎓",
                    TransactionCategory.Travel => "✈️",
                    TransactionCategory.Personal => "👤",
                    TransactionCategory.Other => "📦",
                    _ => "❓"
                };
            }
            
            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
