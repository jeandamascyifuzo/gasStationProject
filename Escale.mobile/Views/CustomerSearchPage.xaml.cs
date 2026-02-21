using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class CustomerSearchPage : ContentPage
{
    public CustomerSearchPage(CustomerSearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
