using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class LuckyPrize : BaseEntity
    {
        public string Title { get; set; }
        public string TitleAr { get; set; }
        public string Description { get; set; }
        public string DescriptionAr { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int ProbabilityWeight { get; set; } // Helps determine chances of winning
        public bool IsActive { get; set; } = true;
    }
}
