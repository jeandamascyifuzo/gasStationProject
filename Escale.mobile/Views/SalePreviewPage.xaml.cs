using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class SalePreviewPage : ContentPage
{
    private readonly SalePreviewViewModel _viewModel;

    public SalePreviewPage(SalePreviewViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel.OnNavigatedTo();
    }
}
