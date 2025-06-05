using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class AddItemWindow : Window
    {
        private int _currentUserId;

        public AddItemWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загружаем категории
                AppConnect.model01.Categories.Load();
                cbCategory.ItemsSource = AppConnect.model01.Categories.Local;
                cbCategory.DisplayMemberPath = "CategoryName";
                cbCategory.SelectedIndex = 0;

                // Загружаем продавцов из таблицы Sellers
                AppConnect.model01.Sellers.Load();
                cbSeller.ItemsSource = AppConnect.model01.Sellers.Local;
                cbSeller.DisplayMemberPath = "SellerName";
                cbSeller.SelectedValuePath = "SellerId";
                cbSeller.SelectedIndex = 0;

                // Загружаем уровни репутации из таблицы ReputationLevels
                AppConnect.model01.ReputationLevels.Load();
                cbReputationLevel.ItemsSource = AppConnect.model01.ReputationLevels.Local;
                cbReputationLevel.DisplayMemberPath = "LevelName";
                cbReputationLevel.SelectedValuePath = "ReputationLevelId";
                cbReputationLevel.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtPrice_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем ввод только цифр, точки и запятой, исключаем пробелы
            if (!Regex.IsMatch(e.Text, @"^[0-9,.]$") || e.Text == " ")
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

            // Проверяем, что точка или запятая вводится только один раз
            if ((newText.Count(c => c == '.') > 1 || newText.Count(c => c == ',') > 1) ||
                (newText.Contains(".") && newText.Contains(",")))
            {
                e.Handled = true;
                return;
            }

            // Проверяем количество символов
            if (newText.Contains("."))
            {
                var parts = newText.Split('.');
                // Ограничиваем до 6 символов перед точкой
                if (parts[0].Length > 6)
                {
                    e.Handled = true;
                    return;
                }
                // Ограничиваем до 2 символов после точки
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    e.Handled = true;
                }
            }
            else if (newText.Contains(","))
            {
                var parts = newText.Split(',');
                // Ограничиваем до 6 символов перед запятой
                if (parts[0].Length > 6)
                {
                    e.Handled = true;
                    return;
                }
                // Ограничиваем до 2 символов после запятой
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    e.Handled = true;
                }
            }
            else
            {
                // Если нет точки/запятой, ограничиваем до 6 символов
                if (newText.Length > 6)
                {
                    e.Handled = true;
                }
            }
        }

        private void txtPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;

            // Заменяем запятую на точку для единообразия
            if (text.Contains(","))
            {
                text = text.Replace(",", ".");
                textBox.Text = text;
                textBox.CaretIndex = text.Length;
            }

            // Удаляем пробелы
            if (text.Contains(" "))
            {
                text = text.Replace(" ", "");
                textBox.Text = text;
                textBox.CaretIndex = text.Length;
            }

            // Проверяем количество символов
            if (text.Contains("."))
            {
                var parts = text.Split('.');
                if (parts[0].Length > 6)
                {
                    textBox.Text = parts[0].Substring(0, 6) + "." + (parts.Length > 1 ? parts[1] : "");
                    textBox.CaretIndex = textBox.Text.Length;
                }
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    textBox.Text = parts[0] + "." + parts[1].Substring(0, 2);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
            else
            {
                // Если нет точки, ограничиваем до 6 символов
                if (text.Length > 6)
                {
                    textBox.Text = text.Substring(0, 6);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtItemName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    MessageBox.Show("Название и цена обязательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем, что цена — валидное положительное число, используя CultureInfo.InvariantCulture
                if (!decimal.TryParse(txtPrice.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price) || price <= 0m)
                {
                    MessageBox.Show("Цена должна быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Дополнительная проверка длины цены
                var priceParts = txtPrice.Text.Split('.');
                if (priceParts[0].Length > 6 || (priceParts.Length > 1 && priceParts[1].Length > 2))
                {
                    MessageBox.Show("Цена должна содержать не более 6 символов до точки и 2 символа после точки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selectedSeller = cbSeller.SelectedItem as Sellers;
                var selectedReputation = cbReputationLevel.SelectedItem as ReputationLevels;
                var selectedCategory = cbCategory.SelectedItem as Categories;

                if (selectedSeller == null || selectedReputation == null || selectedCategory == null)
                {
                    MessageBox.Show("Выберите продавца, уровень репутации и категорию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var item = new Items
                {
                    ItemName = txtItemName.Text,
                    BasePrice = price,
                    Description = txtDescription.Text,
                    SellerId = selectedSeller.SellerId,
                    CategoryId = selectedCategory.CategoryId,
                    RequiredReputationLevelId = selectedReputation.LevelId,
                    ImageUrl = "default.jpg" // Если поле ImageUrl есть в модели
                };

                AppConnect.model01.Items.Add(item);
                AppConnect.model01.SaveChanges();

                MessageBox.Show("Предмет успешно добавлен!");
                this.Close();
            }
            catch (DbUpdateException ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.InnerException?.Message ?? ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbCategory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }

        private void cbSeller_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }

        private void cbReputationLevel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }
    }
}