using Escale.mobile.ViewModels;

namespace Escale.mobile.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            vm.SubscribeToNotifications();
            vm.RefreshCommand.Execute(null);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is DashboardViewModel vm)
        {
            vm.UnsubscribeFromNotifications();
        }
    }
}
