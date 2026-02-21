using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class CustomerInfoPage : ContentPage
{
    public CustomerInfoPage(CustomerInfoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
