using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TerrariaMarketplace.Services;

namespace TerrariaMarketplace.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
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

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnRegisterSuccess;
        public event Action OnBackToLogin;

        public RegisterViewModel(AuthService authService)
        {
            _authService = authService;

            RegisterCommand = new RelayCommand(Register);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        private void Register(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                MessageBox.Show("Please fill all fields");
                return;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Passwords do not match");
                return;
            }

            if (_authService.Register(Username, Email, Password))
            {
                MessageBox.Show("Registration successful! Please login.");
                OnRegisterSuccess?.Invoke();
            }
            else
            {
                MessageBox.Show("Username or email already exists");
            }
        }

        private void BackToLogin(object parameter)
        {
            OnBackToLogin?.Invoke();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}