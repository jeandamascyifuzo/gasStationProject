using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class SaleCompletePage : ContentPage
{
    public SaleCompletePage(SaleCompleteViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
