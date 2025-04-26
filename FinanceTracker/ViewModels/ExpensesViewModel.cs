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
    public class ExpensesViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private ObservableCollection<Transaction> _transactions;
        private Transaction _selectedTransaction;
        private bool _isAddTransactionVisible;
        private string _description;
        private decimal _amount;
        private DateTime _date = DateTime.Now;
        private TransactionType _transactionType = TransactionType.Expense;
        private TransactionCategory _category = TransactionCategory.Food;
        private string _vendor;
        private string _paymentMethod;
        private bool _isRecurring;
        private string _recurrenceFrequency = "Monthly";
        private string _errorMessage;
        private bool _isError;

        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set => SetProperty(ref _selectedTransaction, value);
        }

        public bool IsAddTransactionVisible
        {
            get => _isAddTransactionVisible;
            set => SetProperty(ref _isAddTransactionVisible, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public decimal Amount
        {
            get => _amount;
            set => SetProperty(ref _amount, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public TransactionType TransactionType
        {
            get => _transactionType;
            set => SetProperty(ref _transactionType, value);
        }

        public TransactionCategory Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Vendor
        {
            get => _vendor;
            set => SetProperty(ref _vendor, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetProperty(ref _paymentMethod, value);
        }

        public bool IsRecurring
        {
            get => _isRecurring;
            set => SetProperty(ref _isRecurring, value);
        }

        public string RecurrenceFrequency
        {
            get => _recurrenceFrequency;
            set => SetProperty(ref _recurrenceFrequency, value);
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

        public ICommand RefreshCommand { get; }
        public ICommand AddTransactionCommand { get; }
        public ICommand SaveTransactionCommand { get; }
        public ICommand CancelAddTransactionCommand { get; }
        public ICommand DeleteTransactionCommand { get; }

        public ExpensesViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Expenses";
            _databaseService = databaseService;
            _sessionService = sessionService;
            Transactions = new ObservableCollection<Transaction>();

            RefreshCommand = new Command(async () => await LoadTransactionsAsync());
            AddTransactionCommand = new Command(ShowAddTransaction);
            SaveTransactionCommand = new Command(async () => await SaveTransactionAsync());
            CancelAddTransactionCommand = new Command(HideAddTransaction);
            DeleteTransactionCommand = new Command<Transaction>(async (transaction) => await DeleteTransactionAsync(transaction));
        }

        public async Task LoadTransactionsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var transactions = await _databaseService.GetTransactionsAsync(_sessionService.CurrentUser.Id);
                
                Transactions.Clear();
                foreach (var transaction in transactions.OrderByDescending(t => t.Date))
                {
                    Transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading transactions: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowAddTransaction()
        {
            // Reset form fields
            Description = string.Empty;
            Amount = 0;
            Date = DateTime.Now;
            TransactionType = TransactionType.Expense;
            Category = TransactionCategory.Food;
            Vendor = string.Empty;
            PaymentMethod = string.Empty;
            IsRecurring = false;
            RecurrenceFrequency = "Monthly";
            IsError = false;
            
            IsAddTransactionVisible = true;
        }

        private void HideAddTransaction()
        {
            IsAddTransactionVisible = false;
        }

        private async Task SaveTransactionAsync()
        {
            if (string.IsNullOrWhiteSpace(Description) || Amount <= 0)
            {
                ErrorMessage = "Please enter a description and a valid amount";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var transaction = new Transaction
                {
                    UserId = _sessionService.CurrentUser.Id,
                    Description = Description,
                    Amount = Amount,
                    Date = Date,
                    Type = TransactionType,
                    Category = Category,
                    Vendor = Vendor,
                    PaymentMethod = PaymentMethod,
                    IsRecurring = IsRecurring,
                    RecurrenceFrequency = IsRecurring ? RecurrenceFrequency : null
                };

                await _databaseService.SaveTransactionAsync(transaction);
                
                // Refresh the list
                await LoadTransactionsAsync();
                
                // Hide the form
                HideAddTransaction();
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

        private async Task DeleteTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
                return;

            IsBusy = true;

            try
            {
                await _databaseService.DeleteTransactionAsync(transaction);
                Transactions.Remove(transaction);
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error deleting transaction: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
