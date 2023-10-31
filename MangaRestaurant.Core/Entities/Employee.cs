using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities
{
    public class Employee : BaseEntity
    {
        public string Name { get; set; }    
        public decimal Salary { get; set; }

      //  public DateOnly BirthDate { get; set; }
        
        public Department Department { get; set; }
    }
}
