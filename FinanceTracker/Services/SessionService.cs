using System;
using System.Threading.Tasks;
using FinanceTracker.Models;

namespace FinanceTracker.Services
{
    public class SessionService
    {
        private User _currentUser;
        private readonly DatabaseService _databaseService;

        public SessionService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public User CurrentUser => _currentUser;

        public bool IsLoggedIn => _currentUser != null;

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            UserChanged?.Invoke(this, user);
        }

        public void ClearCurrentUser()
        {
            _currentUser = null;
            UserChanged?.Invoke(this, null);
        }

        public async Task<Settings> GetUserSettingsAsync()
        {
            if (_currentUser == null)
                return null;

            return await _databaseService.GetSettingsAsync(_currentUser.Id);
        }

        public event EventHandler<User> UserChanged;
    }
}
