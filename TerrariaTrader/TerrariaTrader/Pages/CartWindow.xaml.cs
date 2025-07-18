﻿using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class CartWindow : Window
    {
        private int _userId;
        private bool _isAdmin;

        public class CartItemViewModel
        {
            public int CartId { get; set; }
            public CartItems CartItem { get; set; }
            public Items Items { get; set; }
            public int Quantity { get => CartItem.Quantity; set => CartItem.Quantity = value; }
            public decimal TotalPrice { get => CartItem.Quantity * Items.BasePrice; }
        }

        public CartWindow(int userId, bool isAdmin = false)
        {
            InitializeComponent();
            _userId = userId;
            _isAdmin = isAdmin;
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            var cartItems = AppConnect.model01.CartItems
                .Where(ci => ci.UserId == _userId)
                .Include(ci => ci.Items)
                .Select(ci => new CartItemViewModel
                {
                    CartId = ci.CartId,
                    CartItem = ci,
                    Items = ci.Items
                })
                .ToList();
            cartItemsListView.ItemsSource = cartItems;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int cartId = (int)button.Tag;
                var cartItem = AppConnect.model01.CartItems.Find(cartId);
                if (cartItem != null)
                {
                    AppConnect.model01.CartItems.Remove(cartItem);
                    AppConnect.model01.SaveChanges();
                    LoadCartItems();
                    MessageBox.Show("Товар удален из корзины!");
                }
            }
        }

        private void btnUpdateQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int cartId = (int)button.Tag;
                var cartItem = AppConnect.model01.CartItems.Find(cartId);
                if (cartItem != null)
                {
                    var parent = button.Parent as StackPanel;
                    var txtQuantity = parent?.FindName("txtQuantity") as TextBox;
                    if (txtQuantity != null && int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
                    {
                        cartItem.Quantity = quantity;
                        AppConnect.model01.SaveChanges();
                        LoadCartItems();
                        MessageBox.Show("Количество обновлено!");
                    }
                    else
                    {
                        MessageBox.Show("Введите корректное количество!");
                    }
                }
            }
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            var cartItems = AppConnect.model01.CartItems.Where(ci => ci.UserId == _userId).Include(ci => ci.Items).ToList();
            if (cartItems.Any())
            {
                var order = new Orders
                {
                    UserId = _userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(ci => ci.Items.BasePrice * ci.Quantity),
                    Status = "Completed"
                };
                AppConnect.model01.Orders.Add(order);

                foreach (var item in cartItems)
                {
                    order.OrderItems.Add(new OrderItems
                    {
                        ItemId = item.ItemId,
                        SellerId = item.SellerId,
                        Quantity = item.Quantity,
                        PricePurchase = item.Items.BasePrice
                    });
                    AppConnect.model01.CartItems.Remove(item);
                }

                AppConnect.model01.SaveChanges();
                LoadCartItems();
                MessageBox.Show("Покупка завершена! Проверяйте историю.");
            }
            else
            {
                MessageBox.Show("Корзина пуста!");
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_isAdmin)
            {
                var adminWindow = new AdminCatalogWindow(_userId);
                adminWindow.Show();
            }
            else
            {
                var mainWindow = new MainWindow();
                mainWindow._currentUserId = _userId;
                mainWindow.Show();
            }
            this.Close();
        }

        private void btnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_userId, _isAdmin); // Передаем флаг администратора
            historyWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // Убрана строка: _context?.Dispose();
        }
    }
}