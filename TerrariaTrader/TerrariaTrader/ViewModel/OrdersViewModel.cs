using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TerrariaMarketplace.Models;
using TerrariaMarketplace.Services;

namespace TerrariaMarketplace.ViewModels
{
    public class OrdersViewModel : INotifyPropertyChanged
    {
        private readonly MarketService _marketService;

        private ObservableCollection<Order> _orders;
        public ObservableCollection<Order> Orders
        {
            get => _orders;
            set
            {
                _orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }

        public ICommand BackToMainCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnBackToMain;

        public OrdersViewModel(MarketService marketService)
        {
            _marketService = marketService;

            Orders = new ObservableCollection<Order>();
            BackToMainCommand = new RelayCommand(BackToMain);

            LoadOrders();
        }

        private void LoadOrders()
        {
            Orders.Clear();
            var orders = _marketService.GetUserOrders(_marketService.AuthService.CurrentUser.UserID);
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        private void BackToMain(object parameter)
        {
            OnBackToMain?.Invoke();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
