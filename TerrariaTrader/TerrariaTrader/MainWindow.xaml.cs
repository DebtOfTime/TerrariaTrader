using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class MainWindow : Window
    {
        private enum SortDirection { None, Ascending, Descending }
        private SortDirection _currentSortDirection = SortDirection.None;
        private int _currentUserId = 1; // Placeholder for current user ID (replace with actual user authentication logic)

        public MainWindow()
        {
            InitializeComponent();
            LoadItems();
        }

        private void LoadItems()
        {
            AppConnect.model01 = new Entities();
            AppConnect.model01.Items.Load();
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
        }

        private void btnShowCart_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_currentUserId);
            cartWindow.Show();
            this.Close();
        }

        private void btnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_currentUserId);
            historyWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            AppConnect.model01?.Dispose();
        }
    }
}