using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TerrariaTrader.AppData;

namespace TerrariaTrader.Pages
{
    public partial class HistoryWindow : Window
    {
        private int _userId;
        private bool _isAdmin;

        public class OrderViewModel
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            public List<OrderItemViewModel> OrderItems { get; set; }
        }

        public class OrderItemViewModel
        {
            public OrderItems OrderItem { get; set; } // Ссылка на OrderItems для доступа к связанным данным
            public Items Items { get; set; } // Ссылка на Items
            public Sellers Sellers { get; set; } // Ссылка на Sellers
            public int Quantity { get; set; }
            public decimal TotalItemPrice => Quantity * PricePurchase; // Рассчитываемая стоимость
            public decimal PricePurchase => OrderItem.PricePurchase; // Цена за единицу из OrderItems
        }

        public HistoryWindow(int userId, bool isAdmin = false)
        {
            InitializeComponent();
            _userId = userId;
            _isAdmin = isAdmin;
            LoadHistory();
        }

        private void LoadHistory()
        {
            var orders = AppConnect.model01.Orders
                .Where(o => o.UserId == _userId)
                .Include(o => o.OrderItems)
                .Include("OrderItems.Items")
                .Include("OrderItems.Items.Sellers") // Загружаем данные о продавце
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    //OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                    {
                        OrderItem = oi,
                        Items = oi.Items,
                        Sellers = oi.Items.Sellers,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .ToList();

            ordersListView.ItemsSource = orders;
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

        private void btnShowCart_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow(_userId, _isAdmin);
            cartWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}