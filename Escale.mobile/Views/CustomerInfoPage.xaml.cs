using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class CustomerInfoPage : ContentPage
{
    private readonly CustomerInfoViewModel _viewModel;

    public CustomerInfoPage(CustomerInfoViewModel viewModel)
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
