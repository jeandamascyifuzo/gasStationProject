using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class NewSalePage : ContentPage
{
    public NewSalePage(NewSaleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NewSaleViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }
}
