using QRCoder;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TerrariaTrader.AppData;
using TerrariaTrader.Helpers;

namespace TerrariaTrader.Pages
{
    public partial class AdminCatalogWindow : Window
    {
        private enum SortDirection { None, Ascending, Descending }
        private SortDirection _currentSortDirection = SortDirection.None;
        private int _currentUserId;
        private bool _isAdmin = true; // Предполагаем, что администратор всегда true

        public AdminCatalogWindow(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;
            LoadItems();
        }

        private void LoadItems()
        {
            try
            {
                using (var context = new Entities()) // Use local context
                {
                    context.Items.Load(); // Load into local context
                    RefreshItemsDisplay(context.Items.Local.AsQueryable());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке элементов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshItemsDisplay(IQueryable<Items> itemsQuery)
        {
            var searchText = txtSearch.Text.ToLower();

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
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var context = new Entities())
            {
                var itemsQuery = context.Items.Local.AsQueryable();
                RefreshItemsDisplay(itemsQuery);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new Entities())
            {
                var itemsQuery = context.Items.Local.AsQueryable();
                RefreshItemsDisplay(itemsQuery);
            }
        }

        private void btnSortAscending_Click(object sender, RoutedEventArgs e)
        {
            _currentSortDirection = SortDirection.Ascending;
            using (var context = new Entities())
            {
                var itemsQuery = context.Items.Local.AsQueryable();
                RefreshItemsDisplay(itemsQuery);
            }
        }

        private void btnSortDescending_Click(object sender, RoutedEventArgs e)
        {
            _currentSortDirection = SortDirection.Descending;
            using (var context = new Entities())
            {
                var itemsQuery = context.Items.Local.AsQueryable();
                RefreshItemsDisplay(itemsQuery);
            }
        }

        private void btnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int itemId = (int)button.Tag;
                using (var context = new Entities())
                {
                    var item = context.Items.Find(itemId);
                    if (item == null) return;

                    var existingCartItem = context.CartItems
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
                        context.CartItems.Add(cartItem);
                    }

                    context.SaveChanges();
                    MessageBox.Show("Товар добавлен в корзину!");
                }
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

        private void btnAddItem_Click(object sender, RoutedEventArgs e)
        {
            var addItemWindow = new AddItemWindow(_currentUserId);
            addItemWindow.ShowDialog();
            LoadItems();
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = itemsListView.SelectedItem as Items;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите предмет для редактирования.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editItemWindow = new EditItemWindow(selectedItem);
            editItemWindow.ShowDialog();
            LoadItems(); // Reload to refresh
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = itemsListView.SelectedItem as Items;
            if (selectedItem == null)
            {
                MessageBox.Show("Выберите предмет для удаления.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить предмет {selectedItem.ItemName}? Это также удалит его из корзин всех пользователей.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (var context = new Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var itemToDelete = context.Items.Find(selectedItem.ItemId);
                            if (itemToDelete == null)
                            {
                                throw new InvalidOperationException("Предмет не найден в базе данных.");
                            }

                            var relatedCartItems = context.CartItems
                                .Where(ci => ci.ItemId == itemToDelete.ItemId)
                                .ToList();
                            foreach (var cartItem in relatedCartItems)
                            {
                                context.CartItems.Remove(cartItem);
                            }

                            context.Items.Remove(itemToDelete);
                            context.SaveChanges();

                            transaction.Commit();
                            LoadItems(); // Reload to refresh
                            MessageBox.Show("Предмет удалён из каталога и корзин пользователей!");
                        }
                        catch (DbUpdateException ex)
                        {
                            transaction.Rollback();
                            var innerMessage = ex.InnerException?.Message ?? ex.Message;
                            MessageBox.Show($"Ошибка при удалении: {innerMessage}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show($"Неожиданная ошибка: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // No Dispose call to avoid affecting static context
        }

        private void btnGenerateQR_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
    }
}