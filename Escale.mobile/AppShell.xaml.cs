using Escale.mobile.Views;

namespace Escale.mobile
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("CustomerSearch", typeof(CustomerSearchPage));
            Routing.RegisterRoute("SubscriptionCustomerSearch", typeof(SubscriptionCustomerSearchPage));
            Routing.RegisterRoute("CustomerInfo", typeof(CustomerInfoPage));
            Routing.RegisterRoute("SalePreview", typeof(SalePreviewPage));
            Routing.RegisterRoute("SaleComplete", typeof(SaleCompletePage));
            Routing.RegisterRoute("StationSelection", typeof(StationSelectionPage));
        }
    }
}
