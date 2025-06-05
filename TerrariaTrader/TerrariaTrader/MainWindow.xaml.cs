using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class MainWindow : Window
    {
        private enum SortDirection { None, Ascending, Descending }
        private SortDirection _currentSortDirection = SortDirection.None;
        public int _currentUserId; // Placeholder for current user ID
        private bool _isAdmin; // Track admin status

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded; // Ждем загрузки XAML
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadItems();
            RefreshItemsDisplay();
        }

        private void LoadItems()
        {
            try
            {
                // Проверяем, есть ли данные в базе данных, а не в локальном кэше
                if (!AppConnect.model01.Items.Any()) // Прямой запрос к базе
                {
                    var testItems = new[]
                    {
                        new Items { ItemName = "Sword", BasePrice = 10.99m, Description = "Test Sword" },
                        new Items { ItemName = "Shield", BasePrice = 20.99m, Description = "Test Shield" }
                    };
                    foreach (var item in testItems)
                    {
                        // Проверяем, нет ли уже предмета с таким именем
                        if (!AppConnect.model01.Items.Any(i => i.ItemName == item.ItemName))
                        {
                            AppConnect.model01.Items.Add(item);
                        }
                    }
                    AppConnect.model01.SaveChanges(); // Синхронное сохранение
                }
                AppConnect.model01.Items.Load(); // Синхронная загрузка
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки предметов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshItemsDisplay()
        {
            var searchText = txtSearch?.Text?.ToLower() ?? string.Empty;
            var itemsQuery = AppConnect.model01.Items.Local.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                itemsQuery = itemsQuery.Where(item => item.ItemName.ToLower().Contains(searchText) ||
                                                      item.Description.ToLower().Contains(searchText));
            }

            switch (_currentSortDirection)
            {
                case SortDirection.Ascending:
                    itemsQuery = itemsQuery.OrderBy(item => item.ItemName);
                    break;
                case SortDirection.Descending:
                    itemsQuery = itemsQuery.OrderByDescending(item => item.ItemName);
                    break;
            }

            itemsListView.ItemsSource = itemsQuery.ToList();
            itemsListView.DataContext = this; // Set DataContext for IsAdmin binding
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                RefreshItemsDisplay();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            RefreshItemsDisplay();
        }

        private void btnSortAscending_Click(object sender, RoutedEventArgs e)
        {
            _currentSortDirection = SortDirection.Ascending;
            RefreshItemsDisplay();
        }

        private void btnSortDescending_Click(object sender, RoutedEventArgs e)
        {
            _currentSortDirection = SortDirection.Descending;
            RefreshItemsDisplay();
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            try
            {
                int itemId = (int)button.Tag;
                var item = AppConnect.model01.Items.Find(itemId);
                if (item == null)
                {
                    MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingCartItem = AppConnect.model01.CartItems
                    .FirstOrDefault(ci => ci.UserId == _currentUserId && ci.ItemId == itemId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity += 1; // Increment quantity
                }
                else
                {
                    var cartItem = new CartItems
                    {
                        ItemId = itemId,
                        UserId = _currentUserId,
                        SellerId = item.SellerId,
                        Quantity = 1,
                        AddedDate = DateTime.Now
                    };
                    AppConnect.model01.CartItems.Add(cartItem);
                }

                AppConnect.model01.SaveChanges();
                MessageBox.Show("Товар добавлен в корзину!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении в корзину: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnShowCart_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_currentUserId, _isAdmin); // Передаем флаг администратора
            cartWindow.Show();
            this.Close();
        }

        private void btnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_currentUserId, _isAdmin); // Передаем флаг администратора
            historyWindow.Show();
            this.Close();
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления товара в разработке.");
        }

        private async void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Items item)
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() => MessageBox.Show($"Редактирование товара: {item.ItemName}"));
                });
            }
        }

        private async void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is Items item)
            {
                var result = Dispatcher.Invoke(() => MessageBox.Show($"Удалить товар {item.ItemName}?", "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question));

                if (result == MessageBoxResult.Yes)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            AppConnect.model01.Items.Remove(item);
                            AppConnect.model01.SaveChanges();
                            Dispatcher.Invoke(() => RefreshItemsDisplay());
                            Dispatcher.Invoke(() => MessageBox.Show("Товар удален!"));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() => MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error));
                        }
                    });
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // Не очищаем AppConnect.model01 здесь
        }

        public bool IsAdmin => _isAdmin; // Property for XAML binding
    }
}