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
    public class CartViewModel : INotifyPropertyChanged
    {
        private readonly MarketService _marketService;

        private ObservableCollection<CartItem> _cartItems;
        public ObservableCollection<CartItem> CartItems
        {
            get => _cartItems;
            set
            {
                _cartItems = value;
                OnPropertyChanged(nameof(CartItems));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }

        public decimal TotalPrice => CartItems?.Sum(i => i.Quantity * i.MerchantItem.CurrentPrice) ?? 0;

        public ICommand RemoveFromCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand BackToMainCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action OnCheckoutSuccess;
        public event Action OnBackToMain;

        public CartViewModel(MarketService marketService)
        {
            _marketService = marketService;

            CartItems = new ObservableCollection<CartItem>();

            RemoveFromCartCommand = new RelayCommand(RemoveFromCart);
            CheckoutCommand = new RelayCommand(Checkout);
            BackToMainCommand = new RelayCommand(BackToMain);

            LoadCart();
        }

        private void LoadCart()
        {
            CartItems.Clear();
            var cart = _marketService.GetUserCart(_marketService.AuthService.CurrentUser.UserID);
            if (cart?.CartItems != null)
            {
                foreach (var item in cart.CartItems)
                {
                    CartItems.Add(item);
                }
            }
        }

        private void RemoveFromCart(object cartItemId)
        {
            if (_marketService.RemoveFromCart(_marketService.AuthService.CurrentUser.UserID, (int)cartItemId))
            {
                LoadCart();
                MessageBox.Show("Item removed from cart");
            }
            else
            {
                MessageBox.Show("Failed to remove item from cart");
            }
        }

        private void Checkout(object parameter)
        {
            if (!CartItems.Any())
            {
                MessageBox.Show("Your cart is empty");
                return;
            }

            if (_marketService.Checkout(_marketService.AuthService.CurrentUser.UserID))
            {
                MessageBox.Show("Order placed successfully!");
                OnCheckoutSuccess?.Invoke();
            }
            else
            {
                MessageBox.Show("Failed to place order");
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
