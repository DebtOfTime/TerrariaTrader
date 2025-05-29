using System;

namespace TerrariaMarketplace.Models
{
    public class UserMerchantRelation
    {
        public int UserMerchantRelationID { get; set; }
        public int UserID { get; set; }
        public int MerchantID { get; set; }
        public int RelationLevel { get; set; }
        public DateTime? LastInteractionDate { get; set; }

        public virtual User User { get; set; }
        public virtual Merchant Merchant { get; set; }
    }
}
