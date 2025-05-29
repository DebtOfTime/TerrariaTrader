using System.Windows;
using TerrariaMarketplace.ViewModels;

namespace TerrariaMarketplace.Views
{
    public partial class OrdersView : Window
    {
        public OrdersView(OrdersViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnBackToMain += () => Close();
        }
    }
}