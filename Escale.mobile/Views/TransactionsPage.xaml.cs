using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class TransactionsPage : ContentPage
{
    public TransactionsPage(TransactionsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TransactionsViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }
}
