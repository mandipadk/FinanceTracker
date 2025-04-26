using System;

namespace FinanceTracker.Models
{
    public class Budget
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public TransactionCategory Category { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Period { get; set; } // Weekly, Monthly, Yearly
        public decimal CurrentSpending { get; set; }
        public decimal RemainingAmount => Amount - CurrentSpending;
        public double PercentageUsed => (double)(CurrentSpending / Amount) * 100;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
