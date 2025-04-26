using System;

namespace FinanceTracker.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum TransactionCategory
    {
        // Income categories
        Salary,
        Investment,
        Gift,
        
        // Expense categories
        Food,
        Housing,
        Transportation,
        Entertainment,
        Shopping,
        Utilities,
        Healthcare,
        Education,
        Travel,
        Personal,
        Other
    }

    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public TransactionCategory Category { get; set; }
        public DateTime Date { get; set; }
        public string PaymentMethod { get; set; }
        public string Vendor { get; set; }
        public bool IsRecurring { get; set; }
        public string RecurrenceFrequency { get; set; } // Daily, Weekly, Monthly, Yearly
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
