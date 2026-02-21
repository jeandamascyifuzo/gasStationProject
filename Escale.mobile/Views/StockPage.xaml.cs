using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class StockPage : ContentPage
{
    public StockPage(StockViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is StockViewModel vm)
        {
            vm.RefreshCommand.Execute(null);
        }
    }
}
