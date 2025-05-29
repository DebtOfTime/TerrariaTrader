using System.Collections.Generic;
using TerrariaTrader.Models;

namespace TerrariaMarketplace.Models
{
    public class Merchant
    {
        public int MerchantID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int BaseRelationLevel { get; set; }

        public virtual ICollection<MerchantItem> MerchantItems { get; set; }
        public virtual ICollection<UserMerchantRelation> UserMerchantRelations { get; set; }
    }
}