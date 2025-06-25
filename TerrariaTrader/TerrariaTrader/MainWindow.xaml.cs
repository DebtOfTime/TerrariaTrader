using QRCoder;
using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TerrariaTrader.AppData;
using TerrariaTrader.Helpers;

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
                if (!AppConnect.model01.Items.Any())
                {
                    var testItems = new[]
                    {
                        new Items { ItemName = "Sword", BasePrice = 10.99m, Description = "Test Sword" },
                        new Items { ItemName = "Shield", BasePrice = 20.99m, Description = "Test Shield" }
                    };
                    foreach (var item in testItems)
                    {
                        if (!AppConnect.model01.Items.Any(i => i.ItemName == item.ItemName))
                        {
                            AppConnect.model01.Items.Add(item);
                        }
                    }
                    AppConnect.model01.SaveChanges();
                }
                AppConnect.model01.Items.Load();
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
            itemsListView.DataContext = this;
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
                    existingCartItem.Quantity += 1;
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
            var cartWindow = new CartWindow(_currentUserId, _isAdmin);
            cartWindow.Show();
            this.Close();
        }

        private void btnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_currentUserId, _isAdmin);
            historyWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void btnGenerateQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generate a dynamic link with user context
                string qrText = $"https://terraria-trader-app.com?userId={_currentUserId}&isAdmin={_isAdmin}";

                var qrCodeImage = TerrariaQRCodeHelper.GenerateQRCode(qrText);

                var qrWindow = new Window
                {
                    Title = _isAdmin ? "Админ QR-код" : "QR-код приложения",
                    Width = 300,
                    Height = 330,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this
                };

                var stackPanel = new StackPanel { Margin = new Thickness(15) };
                var image = new System.Windows.Controls.Image
                {
                    Source = qrCodeImage,
                    Width = 250,
                    Height = 250,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                var textBlock = new TextBlock
                {
                    Text = _isAdmin
                        ? "Отсканируйте для доступа к админ-панели"
                        : "Отсканируйте для перехода в приложение",
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14
                };

                stackPanel.Children.Add(image);
                stackPanel.Children.Add(textBlock);
                qrWindow.Content = stackPanel;

                qrWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации QR-кода: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool IsAdmin => _isAdmin;
    }
}