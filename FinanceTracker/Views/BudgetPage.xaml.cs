using FinanceTracker.ViewModels;

namespace FinanceTracker.Views;

public partial class BudgetPage : ContentPage
{
    private readonly BudgetViewModel _viewModel;
    
    public BudgetPage(BudgetViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadBudgetsAsync();
    }
}
