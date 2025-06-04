using System;
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
        private Entities _context;

        public class CartItemViewModel
        {
            public int CartId { get; set; }
            public CartItems CartItem { get; set; }
            public Items Items { get; set; }
            public int Quantity { get => CartItem.Quantity; set => CartItem.Quantity = value; }
            public decimal TotalPrice { get => CartItem.Quantity * Items.BasePrice; }
        }

        public CartWindow(int userId)
        {
            InitializeComponent();
            _userId = userId;
            _context = new Entities();
            LoadCartItems();
        }

        private void LoadCartItems()
        {
            var cartItems = _context.CartItems
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
                var cartItem = _context.CartItems.Find(cartId);
                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
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
                var cartItem = _context.CartItems.Find(cartId);
                if (cartItem != null)
                {
                    var parent = button.Parent as StackPanel;
                    var txtQuantity = parent?.FindName("txtQuantity") as TextBox;
                    if (txtQuantity != null && int.TryParse(txtQuantity.Text, out int quantity) && quantity > 0)
                    {
                        cartItem.Quantity = quantity;
                        _context.SaveChanges();
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
            var cartItems = _context.CartItems.Where(ci => ci.UserId == _userId).Include(ci => ci.Items).ToList();
            if (cartItems.Any())
            {
                var order = new Orders
                {
                    UserId = _userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cartItems.Sum(ci => ci.Items.BasePrice * ci.Quantity),
                    Status = "Completed"
                };
                _context.Orders.Add(order);

                foreach (var item in cartItems)
                {
                    order.OrderItems.Add(new OrderItems
                    {
                        ItemId = item.ItemId,
                        SellerId = item.SellerId,
                        Quantity = item.Quantity,
                        PricePurchase = item.Items.BasePrice
                    });
                    _context.CartItems.Remove(item);
                }

                _context.SaveChanges();
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
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void btnShowHistory_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_userId);
            historyWindow.Show();
            this.Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            _context?.Dispose();
        }
    }
}