using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class Autorization : Window
    {
        public Autorization()
        {
            InitializeComponent();
            Application.Current.ShutdownMode = ShutdownMode.OnLastWindowClose;
            Application.Current.MainWindow = this;
        }

        private void txtLogin_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtLogin.Text == "Login")
            {
                txtLogin.Text = "";
                txtLogin.Foreground = Brushes.Black;
            }
        }

        private void txtLogin_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLogin.Text))
            {
                txtLogin.Text = "Login";
                txtLogin.Foreground = Brushes.Gray;
            }
        }

        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (psbPassword.Visibility == Visibility.Visible)
            {
                string password = psbPassword.Password.ToString();
                VisiblePasswordBox.Text = password;
                psbPassword.Visibility = Visibility.Collapsed;
                VisiblePasswordBox.Visibility = Visibility.Visible;
                EyeIcon.Source = new BitmapImage(new Uri("/Images/eyeClose.jpg", UriKind.Relative));
            }
            else
            {
                string password = VisiblePasswordBox.Text;
                psbPassword.Password = password;
                psbPassword.Visibility = Visibility.Visible;
                VisiblePasswordBox.Visibility = Visibility.Collapsed;
                EyeIcon.Source = new BitmapImage(new Uri("/Images/eyeOpen.jpg", UriKind.Relative));
            }
        }

        private void VisiblePasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            psbPassword.Password = VisiblePasswordBox.Text;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void btAutorize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userObj = AppConnect.model01.Users.FirstOrDefault(x => x.Username == txtLogin.Text && x.Password == psbPassword.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользователя нет!", "Ошибка при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    switch (userObj.IsAdmin)
                    {
                        case true:
                            MessageBox.Show("Здравствуйте, Администратор " + userObj.Username + "!",
                                "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            var adminWindow = new AdminCatalogWindow(userObj.UserId);
                            adminWindow.Show();
                            this.Close();
                            break;

                        case false:
                            MessageBox.Show("Здравствуйте, Пользователь " + userObj.Username + "!",
                                "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            var mainWindowUser = new MainWindow { _currentUserId = userObj.UserId };
                            mainWindowUser.Show();
                            this.Close();
                            break;
                        default:
                            MessageBox.Show("Данные не обнаружены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
                            break;
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + " Критическая работа приложения!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void btRegistration_Click(object sender, RoutedEventArgs e)
        {
            new Registration().Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            AppConnect.Dispose();
        }
    }
}