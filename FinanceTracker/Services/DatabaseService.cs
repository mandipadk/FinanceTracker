using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FinanceTracker.Models;
using SQLite;

namespace FinanceTracker.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private static readonly string DatabaseFilename = "financetracker.db3";

        private static readonly SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

        public DatabaseService()
        {
        }

        private async Task InitializeAsync()
        {
            if (_database != null)
                return;

            _database = new SQLiteAsyncConnection(DatabasePath, Flags);
            
            // Create tables
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Transaction>();
            await _database.CreateTableAsync<Budget>();
            await _database.CreateTableAsync<Goal>();
            await _database.CreateTableAsync<Settings>();
        }

        // User operations
        public async Task<List<User>> GetUsersAsync()
        {
            await InitializeAsync();
            return await _database.Table<User>().ToListAsync();
        }

        public async Task<User> GetUserAsync(string id)
        {
            await InitializeAsync();
            return await _database.Table<User>().Where(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            await InitializeAsync();
            return await _database.Table<User>().Where(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<int> SaveUserAsync(User user)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString();
                return await _database.InsertAsync(user);
            }
            else
            {
                user.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(user);
            }
        }

        public async Task<int> DeleteUserAsync(User user)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(user);
        }

        // Transaction operations
        public async Task<List<Transaction>> GetTransactionsAsync(string userId)
        {
            await InitializeAsync();
            return await _database.Table<Transaction>().Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<Transaction> GetTransactionAsync(string id)
        {
            await InitializeAsync();
            return await _database.Table<Transaction>().Where(t => t.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveTransactionAsync(Transaction transaction)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
                return await _database.InsertAsync(transaction);
            }
            else
            {
                transaction.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(transaction);
            }
        }

        public async Task<int> DeleteTransactionAsync(Transaction transaction)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(transaction);
        }

        // Budget operations
        public async Task<List<Budget>> GetBudgetsAsync(string userId)
        {
            await InitializeAsync();
            return await _database.Table<Budget>().Where(b => b.UserId == userId).ToListAsync();
        }

        public async Task<Budget> GetBudgetAsync(string id)
        {
            await InitializeAsync();
            return await _database.Table<Budget>().Where(b => b.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveBudgetAsync(Budget budget)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(budget.Id))
            {
                budget.Id = Guid.NewGuid().ToString();
                return await _database.InsertAsync(budget);
            }
            else
            {
                budget.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(budget);
            }
        }

        public async Task<int> DeleteBudgetAsync(Budget budget)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(budget);
        }

        // Goal operations
        public async Task<List<Goal>> GetGoalsAsync(string userId)
        {
            await InitializeAsync();
            return await _database.Table<Goal>().Where(g => g.UserId == userId).ToListAsync();
        }

        public async Task<Goal> GetGoalAsync(string id)
        {
            await InitializeAsync();
            return await _database.Table<Goal>().Where(g => g.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveGoalAsync(Goal goal)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(goal.Id))
            {
                goal.Id = Guid.NewGuid().ToString();
                return await _database.InsertAsync(goal);
            }
            else
            {
                goal.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(goal);
            }
        }

        public async Task<int> DeleteGoalAsync(Goal goal)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(goal);
        }

        // Settings operations
        public async Task<Settings> GetSettingsAsync(string userId)
        {
            await InitializeAsync();
            return await _database.Table<Settings>().Where(s => s.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<int> SaveSettingsAsync(Settings settings)
        {
            await InitializeAsync();
            if (string.IsNullOrEmpty(settings.Id))
            {
                settings.Id = Guid.NewGuid().ToString();
                return await _database.InsertAsync(settings);
            }
            else
            {
                settings.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(settings);
            }
        }
    }
}
