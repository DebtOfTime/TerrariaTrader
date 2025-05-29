using System.Windows;
using TerrariaMarketplace.ViewModels;

namespace TerrariaMarketplace.Views
{
    public partial class RegisterView : Window
    {
        public RegisterView(RegisterViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnRegisterSuccess += () => DialogResult = true;
            viewModel.OnBackToLogin += () => DialogResult = false;
        }
    }
}
