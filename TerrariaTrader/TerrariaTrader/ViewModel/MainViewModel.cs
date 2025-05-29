using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using TerrariaMarketplace.Models;
using TerrariaMarketplace.Services;

namespace TerrariaMarketplace.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly MarketService _marketService;

        private ObservableCollection<Merchant> _merchants;
        public ObservableCollection<Merchant> Merchants
        {
            get => _merchants;
            set
            {
                _merchants = value;
                OnPropertyChanged(nameof(Merchants));
            }
        }

        private ObservableCollection<MerchantItem> _currentMerchantItems;
        public ObservableCollection<MerchantItem> CurrentMerchantItems
        {
            get => _currentMerchantItems;
            set
            {
                _currentMerchantItems = value;
                OnPropertyChanged(nameof(CurrentMerchantItems));
            }
        }

        private Merchant _selectedMerchant;
        public Merchant SelectedMerchant
        {
            get => _selectedMerchant;
            set
            {
                _selectedMerchant = value;
                OnPropertyChanged(nameof(SelectedMerchant));
                if (value != null)
                {
                    LoadMerchantItems(value.MerchantID);
                }
            }
        }

        public ICommand AddToCartCommand { get; }
        public ICommand ViewCartCommand { get; }
        public ICommand ViewOrdersCommand { get; }
        public ICommand LogoutCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnViewCart;
        public event Action OnViewOrders;
        public event Action OnLogout;

        public MarketService MarketService => _marketService;

        public MainViewModel(MarketService marketService)
        {
            _marketService = marketService;

            Merchants = new ObservableCollection<Merchant>();
            CurrentMerchantItems = new ObservableCollection<MerchantItem>();

            AddToCartCommand = new RelayCommand(AddToCart);
            ViewCartCommand = new RelayCommand(ViewCart);
            ViewOrdersCommand = new RelayCommand(ViewOrders);
            LogoutCommand = new RelayCommand(Logout);

            LoadMerchants();
        }

        private void LoadMerchants()
        {
            Merchants.Clear();
            var merchants = _marketService.GetMerchants();
            foreach (var merchant in merchants)
            {
                Merchants.Add(merchant);
            }

            if (Merchants.Any())
            {
                SelectedMerchant = Merchants.First();
            }
        }

        private void LoadMerchantItems(int merchantId)
        {
            CurrentMerchantItems.Clear();
            var items = _marketService.GetAvailableItems(merchantId);
            foreach (var item in items)
            {
                CurrentMerchantItems.Add(item);
            }
        }

        private void AddToCart(object merchantItemId)
        {
            if (_marketService.AddToCart(_marketService.AuthService.CurrentUser.UserID, (int)merchantItemId))
            {
                MessageBox.Show("Item added to cart");
            }
            else
            {
                MessageBox.Show("Failed to add item to cart");
            }
        }

        private void ViewCart(object parameter)
        {
            OnViewCart?.Invoke();
        }

        private void ViewOrders(object parameter)
        {
            OnViewOrders?.Invoke();
        }

        private void Logout(object parameter)
        {
            OnLogout?.Invoke();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}