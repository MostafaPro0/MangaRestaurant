using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaRestaurant.Core.Entities.Order
{
    public class OrderAddress
    {
        public OrderAddress()
        {

        }
        public OrderAddress(string firstName, string lastName, string city, string country, string street, string state, string zipcode)
        {
            FirstName = firstName;
            LastName = lastName;
            City = city;
            Country = country;
            Street = street;
            State = state;
            ZipCode = zipcode;
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? LocationUrl { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
