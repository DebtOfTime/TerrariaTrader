//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TerrariaTrader.AppData
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderItems
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public int SellerId { get; set; }
        public int Quantity { get; set; }
        public decimal PricePurchase { get; set; }
    
        public virtual Items Items { get; set; }
        public virtual Orders Orders { get; set; }
        public virtual Sellers Sellers { get; set; }
    }
}
