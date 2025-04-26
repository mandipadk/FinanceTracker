using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FinanceTracker.Models;
using FinanceTracker.Services;
using Microsoft.Maui.Controls;

namespace FinanceTracker.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private ObservableCollection<Budget> _budgets;
        private Budget _selectedBudget;
        private bool _isAddBudgetVisible;
        private string _name;
        private decimal _amount;
        private TransactionCategory _category = TransactionCategory.Food;
        private DateTime _startDate = DateTime.Now;
        private DateTime _endDate = DateTime.Now.AddMonths(1);
        private string _period = "Monthly";
        private string _errorMessage;
        private bool _isError;
        private decimal _totalBudget;
        private decimal _totalSpent;
        private decimal _totalRemaining;

        public ObservableCollection<Budget> Budgets
        {
            get => _budgets;
            set => SetProperty(ref _budgets, value);
        }

        public Budget SelectedBudget
        {
            get => _selectedBudget;
            set => SetProperty(ref _selectedBudget, value);
        }

        public bool IsAddBudgetVisible
        {
            get => _isAddBudgetVisible;
            set => SetProperty(ref _isAddBudgetVisible, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public TransactionCategory Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        public string Period
        {
            get => _period;
            set => SetProperty(ref _period, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public decimal TotalBudget
        {
            get => _totalBudget;
            set => SetProperty(ref _totalBudget, value);
        }

        public decimal TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        public decimal TotalRemaining
        {
            get => _totalRemaining;
            set => SetProperty(ref _totalRemaining, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddBudgetCommand { get; }
        public ICommand SaveBudgetCommand { get; }
        public ICommand CancelAddBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }

        public BudgetViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Budget";
            _databaseService = databaseService;
            _sessionService = sessionService;
            Budgets = new ObservableCollection<Budget>();

            RefreshCommand = new Command(async () => await LoadBudgetsAsync());
            AddBudgetCommand = new Command(ShowAddBudget);
            SaveBudgetCommand = new Command(async () => await SaveBudgetAsync());
            CancelAddBudgetCommand = new Command(HideAddBudget);
            DeleteBudgetCommand = new Command<Budget>(async (budget) => await DeleteBudgetAsync(budget));
        }

        public async Task LoadBudgetsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var budgets = await _databaseService.GetBudgetsAsync(_sessionService.CurrentUser.Id);
                
                Budgets.Clear();
                foreach (var budget in budgets.OrderBy(b => b.Category))
                {
                    Budgets.Add(budget);
                }

                // Calculate totals
                TotalBudget = budgets.Sum(b => b.Amount);
                TotalSpent = budgets.Sum(b => b.CurrentSpending);
                TotalRemaining = TotalBudget - TotalSpent;

                // Update spending for each budget
                await UpdateBudgetSpendingAsync();
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading budgets: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateBudgetSpendingAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            try
            {
                // Get all transactions
                var transactions = await _databaseService.GetTransactionsAsync(_sessionService.CurrentUser.Id);
                
                // Filter to expenses only
                var expenses = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

                // Update each budget's current spending
                foreach (var budget in Budgets)
                {
                    // Get expenses for this category within the budget period
                    var categoryExpenses = expenses
                        .Where(t => t.Category == budget.Category && 
                                   t.Date >= budget.StartDate && 
                                   t.Date <= budget.EndDate)
                        .Sum(t => t.Amount);
                    
                    // Update the budget
                    budget.CurrentSpending = categoryExpenses;
                    await _databaseService.SaveBudgetAsync(budget);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error updating budget spending: {ex.Message}");
            }
        }

        private void ShowAddBudget()
        {
            // Reset form fields
            Name = string.Empty;
            Amount = 0;
            Category = TransactionCategory.Food;
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddMonths(1);
            Period = "Monthly";
            IsError = false;
            
            IsAddBudgetVisible = true;
        }

        private void HideAddBudget()
        {
            IsAddBudgetVisible = false;
        }

        private async Task SaveBudgetAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || Amount <= 0)
            {
                ErrorMessage = "Please enter a name and a valid amount";
                IsError = true;
                return;
            }

            if (EndDate <= StartDate)
            {
                ErrorMessage = "End date must be after start date";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var budget = new Budget
                {
                    UserId = _sessionService.CurrentUser.Id,
                    Name = Name,
                    Amount = Amount,
                    Category = Category,
                    StartDate = StartDate,
                    EndDate = EndDate,
                    Period = Period,
                    CurrentSpending = 0
                };

                await _databaseService.SaveBudgetAsync(budget);
                
                // Refresh the list
                await LoadBudgetsAsync();
                
                // Hide the form
                HideAddBudget();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred: {ex.Message}";
                IsError = true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteBudgetAsync(Budget budget)
        {
            if (budget == null)
                return;

            IsBusy = true;

            try
            {
                await _databaseService.DeleteBudgetAsync(budget);
                Budgets.Remove(budget);
                
                // Recalculate totals
                TotalBudget = Budgets.Sum(b => b.Amount);
                TotalSpent = Budgets.Sum(b => b.CurrentSpending);
                TotalRemaining = TotalBudget - TotalSpent;
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error deleting budget: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
