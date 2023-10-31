
using MangaRestaurant.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Specifications.EmployeeSpecs
{
    public class EmployeeWithDepartmentSpecs : BaseSpecifications<Employee>
    {
        public EmployeeWithDepartmentSpecs()
        : base()
        {
            Includes.Add(E => E.Department);
        }
        public EmployeeWithDepartmentSpecs(int id)
      : base(E=>E.Id == id)
        {
            Includes.Add(E => E.Department);
        }
    }
}
