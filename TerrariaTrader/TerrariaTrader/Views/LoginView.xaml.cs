using Microsoft.Win32;
using System.Windows;
using TerrariaMarketplace.ViewModels;

namespace TerrariaMarketplace.Views
{
    public partial class LoginView : Window
    {
        public LoginView(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OnLoginSuccess += () => DialogResult = true;
            viewModel.OnShowRegister += () =>
            {
                var registerView = new RegisterView(new RegisterViewModel(viewModel.AuthService));
                registerView.Owner = this;
                if (registerView.ShowDialog() == true)
                {
                    PasswordBox.Clear();
                }
            };
        }
    }
}
