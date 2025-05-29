using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace TerrariaMarketplace.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public virtual ICollection<UserMerchantRelation> UserMerchantRelations { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
