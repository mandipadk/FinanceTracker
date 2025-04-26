using FinanceTracker.ViewModels;

namespace FinanceTracker.Views;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _viewModel;
    
    public GoalsPage(GoalsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadGoalsAsync();
    }
}
