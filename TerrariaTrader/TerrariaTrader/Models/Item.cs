using System.Collections.Generic;
using TerrariaTrader.Models;

namespace TerrariaMarketplace.Models
{
    public class Item
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; }
        public int RequiredAccessLevelID { get; set; }

        public virtual AccessLevel RequiredAccessLevel { get; set; }
        public virtual ICollection<MerchantItem> MerchantItems { get; set; }
    }
}
