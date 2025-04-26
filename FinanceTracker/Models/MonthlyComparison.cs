using System;

namespace FinanceTracker.Models
{
    public class MonthlyComparison
    {
        public string Month { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Balance => Income - Expenses;
    }
}
