using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class SubscriptionCustomerSearchPage : ContentPage
{
    private readonly SubscriptionCustomerSearchViewModel _viewModel;

    public SubscriptionCustomerSearchPage(SubscriptionCustomerSearchViewModel viewModel)
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
