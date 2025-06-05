using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TerrariaTrader.AppData;

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
            AppConnect.model01.Items.Load(); // Используем существующий контекст
            RefreshItemsDisplay();
        }

        private void RefreshItemsDisplay()
        {
            var searchText = txtSearch.Text.ToLower();
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
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshItemsDisplay();
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
            if (button != null)
            {
                int itemId = (int)button.Tag;
                var item = AppConnect.model01.Items.Find(itemId);
                if (item == null) return;

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
        }

        private void btnShowCart_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_currentUserId, _isAdmin);
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
            MessageBox.Show("Функция добавления предмета в разработке.");
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция редактирования предмета в разработке.");
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

            var result = MessageBox.Show($"Удалить предмет {selectedItem.ItemName}? Это также удалит связанные записи в заказах.",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                using (var transaction = AppConnect.model01.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete related OrderItems
                        var relatedOrderItems = AppConnect.model01.OrderItems
                            .Where(oi => oi.ItemId == selectedItem.ItemId)
                            .ToList();
                        foreach (var orderItem in relatedOrderItems)
                        {
                            AppConnect.model01.OrderItems.Remove(orderItem);
                        }

                        // Delete related CartItems
                        var relatedCartItems = AppConnect.model01.CartItems
                            .Where(ci => ci.ItemId == selectedItem.ItemId)
                            .ToList();
                        foreach (var cartItem in relatedCartItems)
                        {
                            AppConnect.model01.CartItems.Remove(cartItem);
                        }

                        // Remove the item itself
                        AppConnect.model01.Items.Remove(selectedItem);
                        AppConnect.model01.SaveChanges();

                        transaction.Commit();
                        RefreshItemsDisplay();
                        MessageBox.Show("Предмет и связанные записи удалены!");
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // Не очищаем AppConnect.model01 здесь
        }
    }
}