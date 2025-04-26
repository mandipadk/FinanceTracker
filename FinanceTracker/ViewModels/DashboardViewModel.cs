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
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private User _currentUser;
        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _balance;
        private decimal _budgetRemaining;
        private double _budgetPercentage;
        private ObservableCollection<Transaction> _recentTransactions;

        public User CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

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

        public decimal BudgetRemaining
        {
            get => _budgetRemaining;
            set => SetProperty(ref _budgetRemaining, value);
        }

        public double BudgetPercentage
        {
            get => _budgetPercentage;
            set => SetProperty(ref _budgetPercentage, value);
        }

        public ObservableCollection<Transaction> RecentTransactions
        {
            get => _recentTransactions;
            set => SetProperty(ref _recentTransactions, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand GoToExpensesCommand { get; }
        public ICommand GoToBudgetCommand { get; }
        public ICommand GoToGoalsCommand { get; }
        public ICommand GoToStatisticsCommand { get; }
        public ICommand GoToSettingsCommand { get; }

        public DashboardViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Dashboard";
            _databaseService = databaseService;
            _sessionService = sessionService;
            RecentTransactions = new ObservableCollection<Transaction>();

            RefreshCommand = new Command(async () => await LoadDataAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            GoToExpensesCommand = new Command(async () => await Shell.Current.GoToAsync("//expenses"));
            GoToBudgetCommand = new Command(async () => await Shell.Current.GoToAsync("//budget"));
            GoToGoalsCommand = new Command(async () => await Shell.Current.GoToAsync("//goals"));
            GoToStatisticsCommand = new Command(async () => await Shell.Current.GoToAsync("//statistics"));
            GoToSettingsCommand = new Command(async () => await Shell.Current.GoToAsync("//settings"));
        }

        public async Task LoadDataAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                CurrentUser = _sessionService.CurrentUser;

                // Load transactions
                var transactions = await _databaseService.GetTransactionsAsync(CurrentUser.Id);
                
                // Calculate totals
                TotalIncome = transactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);
                
                TotalExpenses = transactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);
                
                Balance = TotalIncome - TotalExpenses;

                // Get recent transactions
                var recent = transactions
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList();
                
                RecentTransactions.Clear();
                foreach (var transaction in recent)
                {
                    RecentTransactions.Add(transaction);
                }

                // Get budget information
                var budgets = await _databaseService.GetBudgetsAsync(CurrentUser.Id);
                var currentMonthBudgets = budgets
                    .Where(b => b.StartDate.Month == DateTime.Now.Month && b.StartDate.Year == DateTime.Now.Year)
                    .ToList();
                
                decimal totalBudget = currentMonthBudgets.Sum(b => b.Amount);
                BudgetRemaining = totalBudget - TotalExpenses;
                BudgetPercentage = totalBudget > 0 ? (double)((totalBudget - BudgetRemaining) / totalBudget * 100) : 0;
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task LogoutAsync()
        {
            _sessionService.ClearCurrentUser();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
