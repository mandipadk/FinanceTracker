using System;

namespace FinanceTracker.Models
{
    public class CategorySummary
    {
        public TransactionCategory Category { get; set; }
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; }
    }
}
