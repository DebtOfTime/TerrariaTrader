using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using TerrariaTrader.AppData;
using System.Collections.Generic;

namespace TerrariaTrader.Pages
{
    public partial class HistoryWindow : Window
    {
        private int _userId;
        private Entities _context;

        public class OrderViewModel
        {
            public Orders Order { get; set; }
            public decimal TotalAmount { get => Order.TotalAmount; }
            //public DateTime OrderDate { get => Order.OrderDate; }
            public List<OrderItems> OrderItems { get; set; } // Changed from IQueryable to List
        }

        public HistoryWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new Entities();
            LoadHistory();
        }

        private void LoadHistory()
        {
            var orders = _context.Orders
                .Where(o => o.UserId == _userId)
                .Include(o => o.OrderItems)
                .Include("OrderItems.Items")
                .Include("OrderItems.Sellers")
                .Select(o => new OrderViewModel
                {
                    Order = o,
                    OrderItems = o.OrderItems.ToList() // Materialize to List
                })
                .ToList();

            ordersListView.ItemsSource = orders;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _context?.Dispose();
        }
    }
}