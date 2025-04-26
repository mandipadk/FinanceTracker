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
    public class GoalsViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private readonly SessionService _sessionService;

        private ObservableCollection<Goal> _goals;
        private Goal _selectedGoal;
        private bool _isAddGoalVisible;
        private string _name;
        private string _description;
        private decimal _targetAmount;
        private decimal _currentAmount;
        private DateTime _startDate = DateTime.Now;
        private DateTime _targetDate = DateTime.Now.AddMonths(6);
        private GoalType _goalType = GoalType.ShortTerm;
        private string _icon = "🎯";
        private string _errorMessage;
        private bool _isError;
        private decimal _totalGoalAmount;
        private decimal _totalSavedAmount;
        private double _overallProgress;

        public ObservableCollection<Goal> Goals
        {
            get => _goals;
            set => SetProperty(ref _goals, value);
        }

        public Goal SelectedGoal
        {
            get => _selectedGoal;
            set => SetProperty(ref _selectedGoal, value);
        }

        public bool IsAddGoalVisible
        {
            get => _isAddGoalVisible;
            set => SetProperty(ref _isAddGoalVisible, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public decimal TargetAmount
        {
            get => _targetAmount;
            set => SetProperty(ref _targetAmount, value);
        }

        public decimal CurrentAmount
        {
            get => _currentAmount;
            set => SetProperty(ref _currentAmount, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime TargetDate
        {
            get => _targetDate;
            set => SetProperty(ref _targetDate, value);
        }

        public GoalType GoalType
        {
            get => _goalType;
            set => SetProperty(ref _goalType, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
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

        public decimal TotalGoalAmount
        {
            get => _totalGoalAmount;
            set => SetProperty(ref _totalGoalAmount, value);
        }

        public decimal TotalSavedAmount
        {
            get => _totalSavedAmount;
            set => SetProperty(ref _totalSavedAmount, value);
        }

        public double OverallProgress
        {
            get => _overallProgress;
            set => SetProperty(ref _overallProgress, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddGoalCommand { get; }
        public ICommand SaveGoalCommand { get; }
        public ICommand CancelAddGoalCommand { get; }
        public ICommand DeleteGoalCommand { get; }
        public ICommand UpdateGoalAmountCommand { get; }

        public GoalsViewModel(DatabaseService databaseService, SessionService sessionService)
        {
            Title = "Savings Goals";
            _databaseService = databaseService;
            _sessionService = sessionService;
            Goals = new ObservableCollection<Goal>();

            RefreshCommand = new Command(async () => await LoadGoalsAsync());
            AddGoalCommand = new Command(ShowAddGoal);
            SaveGoalCommand = new Command(async () => await SaveGoalAsync());
            CancelAddGoalCommand = new Command(HideAddGoal);
            DeleteGoalCommand = new Command<Goal>(async (goal) => await DeleteGoalAsync(goal));
            UpdateGoalAmountCommand = new Command<Goal>(async (goal) => await UpdateGoalAmountAsync(goal));
        }

        public async Task LoadGoalsAsync()
        {
            if (_sessionService.CurrentUser == null)
                return;

            IsBusy = true;

            try
            {
                var goals = await _databaseService.GetGoalsAsync(_sessionService.CurrentUser.Id);
                
                Goals.Clear();
                foreach (var goal in goals.OrderBy(g => g.TargetDate))
                {
                    Goals.Add(goal);
                }

                // Calculate totals
                TotalGoalAmount = goals.Sum(g => g.TargetAmount);
                TotalSavedAmount = goals.Sum(g => g.CurrentAmount);
                OverallProgress = TotalGoalAmount > 0 ? (double)(TotalSavedAmount / TotalGoalAmount * 100) : 0;
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error loading goals: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ShowAddGoal()
        {
            // Reset form fields
            Name = string.Empty;
            Description = string.Empty;
            TargetAmount = 0;
            CurrentAmount = 0;
            StartDate = DateTime.Now;
            TargetDate = DateTime.Now.AddMonths(6);
            GoalType = GoalType.ShortTerm;
            Icon = "🎯";
            IsError = false;
            
            IsAddGoalVisible = true;
        }

        private void HideAddGoal()
        {
            IsAddGoalVisible = false;
        }

        private async Task SaveGoalAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || TargetAmount <= 0)
            {
                ErrorMessage = "Please enter a name and a valid target amount";
                IsError = true;
                return;
            }

            if (TargetDate <= StartDate)
            {
                ErrorMessage = "Target date must be after start date";
                IsError = true;
                return;
            }

            if (CurrentAmount < 0 || CurrentAmount > TargetAmount)
            {
                ErrorMessage = "Current amount must be between 0 and target amount";
                IsError = true;
                return;
            }

            IsBusy = true;
            IsError = false;

            try
            {
                var goal = new Goal
                {
                    UserId = _sessionService.CurrentUser.Id,
                    Name = Name,
                    Description = Description,
                    TargetAmount = TargetAmount,
                    CurrentAmount = CurrentAmount,
                    StartDate = StartDate,
                    TargetDate = TargetDate,
                    Type = GoalType,
                    Icon = Icon
                };

                await _databaseService.SaveGoalAsync(goal);
                
                // Refresh the list
                await LoadGoalsAsync();
                
                // Hide the form
                HideAddGoal();
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

        private async Task DeleteGoalAsync(Goal goal)
        {
            if (goal == null)
                return;

            IsBusy = true;

            try
            {
                await _databaseService.DeleteGoalAsync(goal);
                Goals.Remove(goal);
                
                // Recalculate totals
                TotalGoalAmount = Goals.Sum(g => g.TargetAmount);
                TotalSavedAmount = Goals.Sum(g => g.CurrentAmount);
                OverallProgress = TotalGoalAmount > 0 ? (double)(TotalSavedAmount / TotalGoalAmount * 100) : 0;
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error deleting goal: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateGoalAmountAsync(Goal goal)
        {
            if (goal == null)
                return;

            // Show a prompt to update the current amount
            string result = await Application.Current.MainPage.DisplayPromptAsync(
                "Update Progress",
                $"Current amount saved for {goal.Name}:",
                initialValue: goal.CurrentAmount.ToString(),
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrEmpty(result))
                return;

            if (!decimal.TryParse(result, out decimal newAmount))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please enter a valid amount", "OK");
                return;
            }

            if (newAmount < 0 || newAmount > goal.TargetAmount)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Amount must be between 0 and target amount", "OK");
                return;
            }

            IsBusy = true;

            try
            {
                goal.CurrentAmount = newAmount;
                await _databaseService.SaveGoalAsync(goal);
                
                // Refresh the list
                await LoadGoalsAsync();
            }
            catch (Exception ex)
            {
                // Handle error
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
