using System.Collections.Generic;
using TerrariaTrader.Models;

namespace TerrariaMarketplace.Models
{
    public class MerchantItem
    {
        public int MerchantItemID { get; set; }
        public int MerchantID { get; set; }
        public int ItemID { get; set; }
        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }

        public virtual Merchant Merchant { get; set; }
        public virtual Item Item { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
