using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FinanceTracker.Models;

namespace FinanceTracker.Services
{
    public class AuthService
    {
        private readonly DatabaseService _databaseService;

        public AuthService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<User> LoginAsync(string email, string password)
        {
            var user = await _databaseService.GetUserByEmailAsync(email);
            if (user == null)
                return null;

            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
                return null;

            return user;
        }

        public async Task<User> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            var existingUser = await _databaseService.GetUserByEmailAsync(email);
            if (existingUser != null)
                return null; // User already exists

            var passwordHash = HashPassword(password);
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                FirstName = firstName,
                LastName = lastName,
                Username = email,
                DateOfBirth = DateTime.Now,
                Currency = "USD"
            };

            await _databaseService.SaveUserAsync(user);

            // Create default settings for the user
            var settings = new Settings
            {
                UserId = user.Id,
                Currency = "USD",
                Theme = ThemeOption.System,
                NotificationsEnabled = true,
                BudgetAlerts = true,
                GoalReminders = true,
                MonthlyReports = true,
                Language = "en-US"
            };

            await _databaseService.SaveSettingsAsync(settings);

            return user;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
