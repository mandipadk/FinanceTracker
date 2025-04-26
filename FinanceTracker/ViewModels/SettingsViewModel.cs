using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using FinanceTracker.Models;
using FinanceTracker.Services;
using Microsoft.Maui.Controls;

namespace FinanceTracker.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private string _firstName;
        private string _lastName;
        private string _email;
        private DateTime _dateOfBirth;
        private string _currency;
        private ThemeOption _theme;
        private bool _notificationsEnabled;
        private bool _budgetAlerts;
        private bool _goalReminders;
        private bool _monthlyReports;
        private string _language;
        private string _errorMessage;
        private bool _isError;
        private bool _isSuccess;

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public string Currency
        {
            get => _currency;
            set => SetProperty(ref _currency, value);
        }

        public ThemeOption Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        public bool NotificationsEnabled
        {
            get => _notificationsEnabled;
            set => SetProperty(ref _notificationsEnabled, value);
        }

        public bool BudgetAlerts
        {
            get => _budgetAlerts;
            set => SetProperty(ref _budgetAlerts, value);
        }

        public bool GoalReminders
        {
            get => _goalReminders;
            set => SetProperty(ref _goalReminders, value);
        }

        public bool MonthlyReports
        {
            get => _monthlyReports;
            set => SetProperty(ref _monthlyReports, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
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

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public List<string> Currencies { get; } = new List<string>
        {
            "USD", "EUR", "GBP", "JPY", "CAD", "AUD", "CHF", "CNY", "INR", "BRL"
        };

        public List<string> Languages { get; } = new List<string>
        {
            "en-US", "es-ES", "fr-FR", "de-DE", "it-IT", "ja-JP", "zh-CN", "ru-RU", "pt-BR", "ar-SA"
        };

        public ICommand SaveCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand RefreshCommand { get; }

        public SettingsViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Settings";
            _databaseService = databaseService;
            _sessionService = sessionService;

            SaveCommand = new Command(async () => await SaveSettingsAsync());
            LogoutCommand = new Command(async () => await LogoutAsync());
            RefreshCommand = new Command(async () => await LoadSettingsAsync());
        }

        public async Task LoadSettingsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var user = _sessionService.CurrentUser;
                FirstName = user.FirstName;
                LastName = user.LastName;
                Email = user.Email;
                DateOfBirth = user.DateOfBirth;
                Currency = user.Currency;

                var settings = await _sessionService.GetUserSettingsAsync();
                if (settings != null)
                {
                    Theme = settings.Theme;
                    NotificationsEnabled = settings.NotificationsEnabled;
                    BudgetAlerts = settings.BudgetAlerts;
                    GoalReminders = settings.GoalReminders;
                    MonthlyReports = settings.MonthlyReports;
                    Language = settings.Language;
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SaveSettingsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Please fill in all required fields";
                IsError = true;
                IsSuccess = false;
                return;
            }

            IsBusy = true;
            IsError = false;
            IsSuccess = false;

            try
            {
                // Update user
                var user = _sessionService.CurrentUser;
                user.FirstName = FirstName;
                user.LastName = LastName;
                user.Email = Email;
                user.DateOfBirth = DateOfBirth;
                user.Currency = Currency;
                user.UpdatedAt = DateTime.Now;

                await _databaseService.SaveUserAsync(user);

                // Update settings
                var settings = await _sessionService.GetUserSettingsAsync() ?? new Settings { UserId = user.Id };
                settings.Currency = Currency;
                settings.Theme = Theme;
                settings.NotificationsEnabled = NotificationsEnabled;
                settings.BudgetAlerts = BudgetAlerts;
                settings.GoalReminders = GoalReminders;
                settings.MonthlyReports = MonthlyReports;
                settings.Language = Language;
                settings.UpdatedAt = DateTime.Now;

                await _databaseService.SaveSettingsAsync(settings);

                // Update session
                _sessionService.SetCurrentUser(user);

                IsSuccess = true;
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

        private async Task LogoutAsync()
        {
            _sessionService.ClearCurrentUser();
            await Shell.Current.GoToAsync("//login");
        }
    }
}
