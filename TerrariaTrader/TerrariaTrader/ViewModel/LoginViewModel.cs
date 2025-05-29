using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TerrariaMarketplace.Services;

namespace TerrariaMarketplace.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService;

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnLoginSuccess;
        public event Action OnShowRegister;

        public AuthService AuthService => _authService;

        public LoginViewModel(AuthService authService)
        {
            _authService = authService;

            LoginCommand = new RelayCommand(Login);
            RegisterCommand = new RelayCommand(Register);
        }

        private void Login(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            if (_authService.Login(Username, Password))
            {
                OnLoginSuccess?.Invoke();
            }
            else
            {
                MessageBox.Show("Invalid username or password");
            }
        }

        private void Register(object parameter)
        {
            OnShowRegister?.Invoke();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}