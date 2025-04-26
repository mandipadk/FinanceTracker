using System;

namespace FinanceTracker.Models
{
    public enum ThemeOption
    {
        Light,
        Dark,
        System
    }

    public class Settings
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; }
        public string Currency { get; set; } = "USD";
        public ThemeOption Theme { get; set; } = ThemeOption.System;
        public bool NotificationsEnabled { get; set; } = true;
        public bool BudgetAlerts { get; set; } = true;
        public bool GoalReminders { get; set; } = true;
        public bool MonthlyReports { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
