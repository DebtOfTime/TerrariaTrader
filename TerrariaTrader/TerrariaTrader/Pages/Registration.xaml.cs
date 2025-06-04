using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
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
        private void txtEmail_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtEmail.Text == "Email")
            {
                txtEmail.Text = "";
                txtEmail.Foreground = Brushes.Black;
            }
        }
        private void txtEmail_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                txtEmail.Text = "Email";
                txtEmail.Foreground = Brushes.Gray;
            }
        }
        private void psbPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (psbPassword.Password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов!!!");
            }
        }
        private void VisiblePasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (VisiblePasswordBox.Text.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов!!!");
            }
            else
            {
                psbPassword.Password = VisiblePasswordBox.Text;
            }
        }
        private void VisiblePasswordBoxCheck_LostFocus(object sender, RoutedEventArgs e)
        {
            psbPasswordCheck.Password = VisiblePasswordBoxCheck.Text;
        }
        private void TogglePasswordVisibility(object sender, RoutedEventArgs e)
        {
            if (psbPassword.Visibility == Visibility.Visible)
            {
                string password= psbPassword.Password.ToString();
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
                VisiblePasswordBox.Visibility= Visibility.Collapsed;
                EyeIcon.Source = new BitmapImage(new Uri("/Images/eyeOpen.jpg", UriKind.Relative));
            }
        }
        private void TogglePasswordVisibilityCheck(object sender, RoutedEventArgs e)
        {
            if (psbPasswordCheck.Visibility == Visibility.Visible)
            {
                string password = psbPasswordCheck.Password.ToString();
                VisiblePasswordBoxCheck.Text = password;
                psbPasswordCheck.Visibility = Visibility.Collapsed;
                VisiblePasswordBoxCheck.Visibility = Visibility.Visible;
                EyeIconCheck.Source = new BitmapImage(new Uri("/Images/eyeClose.jpg", UriKind.Relative));
            }
            else
            {
                string password = VisiblePasswordBoxCheck.Text;
                psbPasswordCheck.Password = password;
                psbPasswordCheck.Visibility = Visibility.Visible;
                VisiblePasswordBoxCheck.Visibility = Visibility.Collapsed;
                EyeIconCheck.Source = new BitmapImage(new Uri("/Images/eyeOpen.jpg", UriKind.Relative));
            }
        }

        private void Registration1_Click(object sender, RoutedEventArgs e)
        {
            if (txtLogin.Text.Length == 0 || txtLogin.Text == "Login")
            {
                MessageBox.Show("Логин не может быть пустым!!!");
                return;
            }
            if (txtEmail.Text.Length == 0 || txtEmail.Text == "Email")
            {
                MessageBox.Show("Email не может быть пустым!!!");
                return;
            }
            if (IsValidEmail(txtEmail.Text) == false)
            {
                MessageBox.Show("Некорректный Email!!!");
                return;
            }
            if (psbPassword.Password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов!!!");
                return;
            }
            if (AppConnect.model01.Users.Count(x => x.Username == txtLogin.Text) > 0)
            {
                MessageBox.Show("Пользователь с таким логином есть!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information); return;
            }

            if (psbPassword.Password == psbPasswordCheck.Password)
            {
                try
                {
                    Users userObj = new Users()
                    {
                        Username = txtLogin.Text,
                        Email = txtEmail.Text,
                        Password = psbPassword.Password,
                        IsAdmin = false
                    };
                    AppConnect.model01.Users.Add(userObj);
                    AppConnect.model01.SaveChanges();
                    MessageBox.Show("Данные успешно добавлены!",
                    "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch
                {
                    MessageBox.Show("Ошибка при добавлении данных!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Пароли не совпадают!!!");
            }
        }
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void txtEmail_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            var textBox = (TextBox)sender;
            string email = textBox.Text;

            if (!IsValidEmail(email))
            {
                textBox.Background = Brushes.LightPink;
            }
            else
            {
                textBox.Background = Brushes.White;
            }
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void txtLogin_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
