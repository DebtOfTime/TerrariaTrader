using System;
using System.Collections.Generic;
using TerrariaTrader.Models;

namespace TerrariaMarketplace.Models
{
    public class Cart
    {
        public int CartID { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }
    }
}
