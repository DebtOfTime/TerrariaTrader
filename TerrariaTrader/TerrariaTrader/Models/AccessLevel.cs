using System.Collections.Generic;

namespace TerrariaMarketplace.Models
{
    public class AccessLevel
    {
        public int AccessLevelID { get; set; }
        public string LevelName { get; set; }
        public int MinRelationLevel { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Item> Items { get; set; }
    }
}
