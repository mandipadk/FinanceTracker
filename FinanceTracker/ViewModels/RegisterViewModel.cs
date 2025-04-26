using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinanceTracker.Services;
using Microsoft.Maui.Controls;

namespace FinanceTracker.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly SessionService _sessionService;

        private string _firstName;
        private string _lastName;
        private string _email;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private bool _isError;

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

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
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

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel(AuthService authService, SessionService sessionService)
        {
            Title = "Register";
            _authService = authService;
            _sessionService = sessionService;

            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await GoToLoginAsync());
        }

        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ErrorMessage = "Please fill in all fields";
                IsError = true;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var user = await _authService.RegisterAsync(Email, Password, FirstName, LastName);
                if (user != null)
                {
                    _sessionService.SetCurrentUser(user);
                    // Navigate to Dashboard
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else
                {
                    ErrorMessage = "Email already in use";
                    IsError = true;
                }
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

        private async Task GoToLoginAsync()
        {
            // Navigate back to Login page
            await Shell.Current.GoToAsync("..");
        }
    }
}
