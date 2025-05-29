using System.Windows;
using TerrariaMarketplace.ViewModels;

namespace TerrariaMarketplace.Views
{
    public partial class CartView : Window
    {
        public CartView(CartViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnCheckoutSuccess += () => Close();
            viewModel.OnBackToMain += () => Close();
        }
    }
}
