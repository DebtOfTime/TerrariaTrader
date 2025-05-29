using System.Windows;
using TerrariaMarketplace.ViewModels;

namespace TerrariaMarketplace.Views
{
    public partial class MainView : Window
    {
        public MainView(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnViewCart += () =>
            {
                var cartView = new CartView(new CartViewModel(viewModel.MarketService));
                cartView.Owner = this;
                cartView.ShowDialog();
            };

            viewModel.OnViewOrders += () =>
            {
                var ordersView = new OrdersView(new OrdersViewModel(viewModel.MarketService));
                ordersView.Owner = this;
                ordersView.ShowDialog();
            };

            viewModel.OnLogout += () => Close();
        }
    }
}
