using System;
using System.Collections.Generic;
using TerrariaTrader.Models;

namespace TerrariaMarketplace.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int UserID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}