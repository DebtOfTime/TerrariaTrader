using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using TerrariaTrader.AppData;
using TextAlignment = iText.Layout.Properties.TextAlignment;

namespace TerrariaTrader.Pages
{
    public partial class HistoryWindow : Window
    {
        private int _userId;
        private bool _isAdmin;

        public class OrderViewModel
        {
            public int OrderId { get; set; }
            public DateTime? OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string Status { get; set; }
            public List<OrderItemViewModel> OrderItems { get; set; }
        }

        public class OrderItemViewModel
        {
            public OrderItems OrderItem { get; set; }
            public Items Items { get; set; }
            public Sellers Sellers { get; set; }
            public int Quantity { get; set; }
            public decimal TotalItemPrice { get; set; }
            public decimal PricePurchase { get; set; }

            public OrderItemViewModel(OrderItems orderItem, Items items, Sellers sellers, int quantity)
            {
                OrderItem = orderItem;
                Items = items;
                Sellers = sellers;
                Quantity = quantity;
                PricePurchase = orderItem?.PricePurchase ?? 0m;
                TotalItemPrice = Quantity * PricePurchase;
            }
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
            try
            {
                using (var context = new Entities())
                {
                    var orders = context.Orders
                        .Where(o => o.UserId == _userId)
                        .Include(o => o.OrderItems)
                        .Include(o => o.OrderItems.Select(oi => oi.Items))
                        .Include(o => o.OrderItems.Select(oi => oi.Items.Sellers))
                        .ToList()
                        .Select(o => new OrderViewModel
                        {
                            OrderId = o.OrderId,
                            OrderDate = o.OrderDate,
                            TotalAmount = o.TotalAmount,
                            Status = o.Status,
                            OrderItems = o.OrderItems?.Select(oi => new OrderItemViewModel(oi, oi.Items, oi.Items?.Sellers, oi.Quantity)).ToList() ?? new List<OrderItemViewModel>()
                        })
                        .ToList();

                    ordersListView.ItemsSource = orders.Any() ? orders : new List<OrderViewModel>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке истории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnExportToPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new Entities())
                {
                    var orders = context.Orders
                        .Where(o => o.UserId == _userId)
                        .Include(o => o.OrderItems)
                        .Include(o => o.OrderItems.Select(oi => oi.Items))
                        .Include(o => o.OrderItems.Select(oi => oi.Items.Sellers))
                        .ToList();

                    if (orders == null || !orders.Any())
                    {
                        MessageBox.Show("Нет данных для выгрузки в PDF.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    if (!Directory.Exists(desktopPath))
                    {
                        throw new IOException("Рабочий стол не доступен. Запустите приложение от имени администратора.");
                    }
                    string pdfFilePath = Path.Combine(desktopPath, $"PurchaseHistory_{_userId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

                    // Ensure directory exists and handle file lock
                    Directory.CreateDirectory(desktopPath);
                    if (File.Exists(pdfFilePath))
                    {
                        try { File.Delete(pdfFilePath); } catch (IOException) { /* Ignore if still locked */ }
                    }

                    using (var writer = new PdfWriter(pdfFilePath))
                    using (var pdf = new PdfDocument(writer))
                    using (var document = new Document(pdf))
                    {
                        document.SetMargins(20, 20, 20, 20);

                        document.Add(new Paragraph()
                            .Add(new Text("История покупок ID: ").SetFontSize(14))
                            .Add(new Text(_userId.ToString()).SetFontSize(16)));

                        document.Add(new Paragraph($"Дата: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .SetFontSize(12)
                            .SetTextAlignment(TextAlignment.CENTER));

                        foreach (var order in orders)
                        {
                            if (order == null) continue;
                            document.Add(new Paragraph($"Заказ #{order.OrderId}"));
                            if (order.OrderDate.HasValue)
                            {
                                document.Add(new Paragraph($"Дата: {order.OrderDate.Value:dd/MM/yyyy HH:mm}"));
                            }
                            document.Add(new Paragraph($"Сумма: {order.TotalAmount:F2} руб"));
                            if (order.OrderItems != null)
                            {
                                foreach (var item in order.OrderItems)
                                {
                                    if (item == null || item.Items == null) continue;
                                    decimal price = item.PricePurchase;
                                    try
                                    {
                                        document.Add(new Paragraph($"- Товар: {item.Items.ItemName ?? "Неизвестно"}, Количество: {item.Quantity}, " +
                                            $"Цена: {price:F2} руб, Итого: {item.Quantity * price:F2} руб"));
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show($"Ошибка при добавлении элемента в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                document.Add(new Paragraph("Нет элементов заказа."));
                            }
                            document.Add(new Paragraph("------------------------"));
                        }
                    }

                    MessageBox.Show($"PDF сохранен: {pdfFilePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выгрузке в PDF: {ex.Message}\nПодробности: {ex.StackTrace}\nВнутренняя ошибка: {(ex.InnerException != null ? ex.InnerException.Message : "Нет")}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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