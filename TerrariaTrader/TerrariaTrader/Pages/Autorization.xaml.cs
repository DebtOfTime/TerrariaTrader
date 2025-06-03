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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    /// <summary>
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Autorization : Window
    {
        public Autorization()
        {
            InitializeComponent();
            AppConnect.model01 = new Entities2();
        }

        private void btAutorize_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userObj = AppConnect.model01.Users.FirstOrDefault(x => x.Username == txtLogin.Text && x.Password == psbPassword.Password);
                if (userObj == null)
                {
                    MessageBox.Show("Такого пользолвателя нет!", "Ошибуа при авторизации!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    switch (userObj.IsAdmin)
                    {
                        case true:
                            MessageBox.Show("Здравствуйте, Администратор " + userObj.Username + "!",
                        "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                            //NavigationService.Navigate(new Resepts());
                            break;

                        case false:
                            MessageBox.Show("Здравствуйте, Пользователь " + userObj.Username + "!",
                            "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information); break;
                        default: MessageBox.Show("Данные не обнарyжены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning); break;
                    }
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show("Ошибка " + Ex.Message.ToString() + "Критическая работа приложения!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
