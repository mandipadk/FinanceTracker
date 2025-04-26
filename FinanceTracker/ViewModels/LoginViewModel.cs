using System;
using System.Threading.Tasks;
using System.Windows.Input;
using FinanceTracker.Services;
using Microsoft.Maui.Controls;

namespace FinanceTracker.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly SessionService _sessionService;

        private string _email;
        private string _password;
        private string _errorMessage;
        private bool _isError;

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

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel(AuthService authService, SessionService sessionService)
        {
            Title = "Login";
            _authService = authService;
            _sessionService = sessionService;

            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await GoToRegisterAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter email and password";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var user = await _authService.LoginAsync(Email, Password);
                if (user != null)
                {
                    _sessionService.SetCurrentUser(user);
                    // Navigate to Dashboard
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else
                {
                    ErrorMessage = "Invalid email or password";
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

        private async Task GoToRegisterAsync()
        {
            // Navigate to Register page
            await Shell.Current.GoToAsync("register");
        }
    }
}
