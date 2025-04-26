using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinanceTracker.Models;
using FinanceTracker.Services;
using Microsoft.Maui.Controls;

namespace FinanceTracker.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _balance;
        private ObservableCollection<CategorySummary> _expensesByCategory;
        private ObservableCollection<MonthlyComparison> _monthlyComparison;
        private string _selectedTimeFrame = "This Month";
        private DateTime _startDate;
        private DateTime _endDate;

        public decimal TotalIncome
        {
            get => _totalIncome;
            set => SetProperty(ref _totalIncome, value);
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set => SetProperty(ref _totalExpenses, value);
        }

        public decimal Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public ObservableCollection<CategorySummary> ExpensesByCategory
        {
            get => _expensesByCategory;
            set => SetProperty(ref _expensesByCategory, value);
        }

        public ObservableCollection<MonthlyComparison> MonthlyComparison
        {
            get => _monthlyComparison;
            set => SetProperty(ref _monthlyComparison, value);
        }

        public string SelectedTimeFrame
        {
            get => _selectedTimeFrame;
            set
            {
                if (SetProperty(ref _selectedTimeFrame, value))
                {
                    UpdateDateRange();
                    RefreshCommand.Execute(null);
                }
            }
        }

        public List<string> TimeFrames { get; } = new List<string>
        {
            "This Month",
            "Last Month",
            "Last 3 Months",
            "Last 6 Months",
            "This Year",
            "Last Year",
            "All Time"
        };

        public ICommand RefreshCommand { get; }
        public ICommand ExportDataCommand { get; }

        public StatisticsViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Statistics";
            _databaseService = databaseService;
            _sessionService = sessionService;
            ExpensesByCategory = new ObservableCollection<CategorySummary>();
            MonthlyComparison = new ObservableCollection<MonthlyComparison>();

            RefreshCommand = new Command(async () => await LoadStatisticsAsync());
            ExportDataCommand = new Command(async () => await ExportDataAsync());

            // Initialize date range
            UpdateDateRange();
        }

        private void UpdateDateRange()
        {
            var now = DateTime.Now;
            
            switch (SelectedTimeFrame)
            {
                case "This Month":
                    _startDate = new DateTime(now.Year, now.Month, 1);
                    _endDate = _startDate.AddMonths(1).AddDays(-1);
                    break;
                case "Last Month":
                    _startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                    _endDate = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                    break;
                case "Last 3 Months":
                    _startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-3);
                    _endDate = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                    break;
                case "Last 6 Months":
                    _startDate = new DateTime(now.Year, now.Month, 1).AddMonths(-6);
                    _endDate = new DateTime(now.Year, now.Month, 1).AddDays(-1);
                    break;
                case "This Year":
                    _startDate = new DateTime(now.Year, 1, 1);
                    _endDate = new DateTime(now.Year, 12, 31);
                    break;
                case "Last Year":
                    _startDate = new DateTime(now.Year - 1, 1, 1);
                    _endDate = new DateTime(now.Year - 1, 12, 31);
                    break;
                case "All Time":
                    _startDate = DateTime.MinValue;
                    _endDate = DateTime.MaxValue;
                    break;
            }
        }

        public async Task LoadStatisticsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var transactions = await _databaseService.GetTransactionsAsync(_sessionService.CurrentUser.Id);
                
                // Filter transactions by date range
                var filteredTransactions = transactions.Where(t => t.Date >= _startDate && t.Date <= _endDate).ToList();
                
                // Calculate totals
                TotalIncome = filteredTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                TotalExpenses = filteredTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                Balance = TotalIncome - TotalExpenses;

                // Calculate expenses by category
                var expensesByCategory = filteredTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .GroupBy(t => t.Category)
                    .Select(g => new CategorySummary
                    {
                        Category = g.Key,
                        Amount = g.Sum(t => t.Amount),
                        Percentage = TotalExpenses > 0 ? (double)(g.Sum(t => t.Amount) / TotalExpenses * 100) : 0,
                        Color = GetColorForCategory(g.Key)
                    })
                    .OrderByDescending(c => c.Amount)
                    .ToList();

                ExpensesByCategory.Clear();
                foreach (var category in expensesByCategory)
                {
                    ExpensesByCategory.Add(category);
                }

                // Calculate monthly comparison
                var monthlyData = new List<MonthlyComparison>();
                
                // Determine how many months to show based on the selected time frame
                int monthsToShow = 12; // Default to 12 months
                
                if (SelectedTimeFrame == "This Month" || SelectedTimeFrame == "Last Month")
                {
                    monthsToShow = 3; // Show 3 months for short time frames
                }
                else if (SelectedTimeFrame == "Last 3 Months")
                {
                    monthsToShow = 6; // Show 6 months for 3-month time frame
                }
                
                // Get data for the last N months
                for (int i = 0; i < monthsToShow; i++)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    var monthStart = new DateTime(month.Year, month.Month, 1);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    
                    var monthTransactions = transactions.Where(t => t.Date >= monthStart && t.Date <= monthEnd).ToList();
                    
                    var monthlyComparison = new MonthlyComparison
                    {
                        Month = monthStart.ToString("MMM yyyy"),
                        Income = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                        Expenses = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                    };
                    
                    monthlyData.Add(monthlyComparison);
                }
                
                // Reverse the list to show oldest to newest
                monthlyData.Reverse();
                
                MonthlyComparison.Clear();
                foreach (var month in monthlyData)
                {
                    MonthlyComparison.Add(month);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading statistics: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private string GetColorForCategory(TransactionCategory category)
        {
            return category switch
            {
                TransactionCategory.Food => "#FF9800", // Orange
                TransactionCategory.Housing => "#2196F3", // Blue
                TransactionCategory.Transportation => "#4CAF50", // Green
                TransactionCategory.Entertainment => "#9C27B0", // Purple
                TransactionCategory.Shopping => "#E91E63", // Pink
                TransactionCategory.Utilities => "#00BCD4", // Cyan
                TransactionCategory.Healthcare => "#F44336", // Red
                TransactionCategory.Education => "#3F51B5", // Indigo
                TransactionCategory.Travel => "#009688", // Teal
                TransactionCategory.Personal => "#795548", // Brown
                _ => "#607D8B" // Blue Grey (for Other and any other categories)
            };
        }

        private async Task ExportDataAsync()
        {
            // This would be implemented to export data to CSV or other format
            await Application.Current.MainPage.DisplayAlert("Export Data", "This feature is not yet implemented.", "OK");
        }
    }
}
