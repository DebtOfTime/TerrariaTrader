using System;

namespace TerrariaMarketplace.Models
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int MerchantItemID { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedDate { get; set; }

        public virtual Cart Cart { get; set; }
        public virtual MerchantItem MerchantItem { get; set; }
    }
}