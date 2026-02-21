using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class StationSelectionPage : ContentPage
{
    public StationSelectionPage(StationSelectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
