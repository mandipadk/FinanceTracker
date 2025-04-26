using System;

namespace FinanceTracker.Models
{
    public enum GoalType
    {
        ShortTerm,
        MediumTerm,
        LongTerm
    }

    public class Goal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime TargetDate { get; set; }
        public GoalType Type { get; set; }
        public string Icon { get; set; }
        public bool IsCompleted => CurrentAmount >= TargetAmount;
        public double ProgressPercentage => (double)(CurrentAmount / TargetAmount) * 100;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
