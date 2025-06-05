using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using TerrariaTrader.AppData;
using System.Data.Entity.Infrastructure;

namespace TerrariaTrader.Pages
{
    public partial class EditItemWindow : Window
    {
        private readonly Items _itemToEdit;

        public EditItemWindow(Items itemToEdit)
        {
            InitializeComponent();
            _itemToEdit = itemToEdit ?? throw new ArgumentNullException(nameof(itemToEdit));
            LoadData();
            PopulateFields();
        }

        private void LoadData()
        {
            try
            {
                // Загружаем категории
                AppConnect.model01.Categories.Load();
                cbCategory.ItemsSource = AppConnect.model01.Categories.Local;
                cbCategory.DisplayMemberPath = "CategoryName";
                cbCategory.SelectedValuePath = "CategoryId";

                // Загружаем продавцов
                AppConnect.model01.Sellers.Load();
                cbSeller.ItemsSource = AppConnect.model01.Sellers.Local;
                cbSeller.DisplayMemberPath = "SellerName";
                cbSeller.SelectedValuePath = "SellerId";

                // Загружаем уровни репутации
                AppConnect.model01.ReputationLevels.Load();
                cbReputationLevel.ItemsSource = AppConnect.model01.ReputationLevels.Local;
                cbReputationLevel.DisplayMemberPath = "LevelName";
                cbReputationLevel.SelectedValuePath = "ReputationLevelId";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateFields()
        {
            try
            {
                if (_itemToEdit == null) throw new InvalidOperationException("Предмет для редактирования не задан.");

                txtItemName.Text = _itemToEdit.ItemName ?? string.Empty;
                txtPrice.Text = _itemToEdit.BasePrice.ToString("F2", CultureInfo.InvariantCulture);
                txtDescription.Text = _itemToEdit.Description ?? string.Empty;

                // Выбираем категорию
                cbCategory.SelectedValue = _itemToEdit.CategoryId;

                // Выбираем продавца
                cbSeller.SelectedValue = _itemToEdit.SellerId;

                // Выбираем уровень репутации
                cbReputationLevel.SelectedValue = _itemToEdit.RequiredReputationLevelId;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка заполнения полей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (parts[0].Length > 6)
                {
                    e.Handled = true;
                    return;
                }
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    e.Handled = true;
                }
            }
            else if (newText.Contains(","))
            {
                var parts = newText.Split(',');
                if (parts[0].Length > 6)
                {
                    e.Handled = true;
                    return;
                }
                if (parts.Length > 1 && parts[1].Length > 2)
                {
                    e.Handled = true;
                }
            }
            else
            {
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

            if (text.Contains(","))
            {
                text = text.Replace(",", ".");
                textBox.Text = text;
                textBox.CaretIndex = text.Length;
            }

            if (text.Contains(" "))
            {
                text = text.Replace(" ", "");
                textBox.Text = text;
                textBox.CaretIndex = text.Length;
            }

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
                if (text.Length > 6)
                {
                    textBox.Text = text.Substring(0, 6);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtItemName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
                {
                    MessageBox.Show("Название и цена обязательны!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price) || price <= 0m)
                {
                    MessageBox.Show("Цена должна быть положительным числом!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

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

                // Обновляем данные предмета
                _itemToEdit.ItemName = txtItemName.Text;
                _itemToEdit.BasePrice = price;
                _itemToEdit.Description = txtDescription.Text;
                _itemToEdit.SellerId = selectedSeller.SellerId;
                _itemToEdit.CategoryId = selectedCategory.CategoryId;
                _itemToEdit.RequiredReputationLevelId = selectedReputation.LevelId;

                AppConnect.model01.SaveChanges();

                MessageBox.Show("Предмет успешно обновлён!");
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

        private void cbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }

        private void cbSeller_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }

        private void cbReputationLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно добавить логику обновления, если нужно
        }
    }
}