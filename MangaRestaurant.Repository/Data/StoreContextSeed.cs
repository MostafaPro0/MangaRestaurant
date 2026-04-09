using MangaRestaurant.Core.Entities;
using MangaRestaurant.Core.Entities.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MangaRestaurant.Repository.Data
{
    public static class StoreContextSeed
    {
        public async static Task SeedAsync(StoreContext _dbContext)
        {
            if (_dbContext.ProductBrands.Count() == 0)
            {
                var brandData = File.ReadAllText("../MangaRestaurant.Repository/Data/DataSeed/brands.json");
                var brands = JsonSerializer.Deserialize<List<ProductBrand>>(brandData);

                if (brands?.Count() > 0)
                {
                    ////الحل ده بنمسح الاى دى علشان ميضربش ايرور او نمسحه من الملف احسن
                    //brands = brands.Select(b => new ProductBrand()
                    //{
                    //    Name = b.Name,

                    //}).ToList();

                    foreach (var brand in brands)
                    {
                        _dbContext.Set<ProductBrand>().Add(brand);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            if (_dbContext.ProductCategories.Count() == 0)
            {
                var categoryData = File.ReadAllText("../MangaRestaurant.Repository/Data/DataSeed/categories.json");
                var categories = JsonSerializer.Deserialize<List<ProductCategory>>(categoryData);

                if (categories?.Count() > 0)
                {
                    ////الحل ده بنمسح الاى دى علشان ميضربش ايرور او نمسحه من الملف احسن
                    //categories = categories.Select(b => new ProductCategory()
                    //{
                    //    Name = b.Name,

                    //}).ToList();

                    foreach (var category in categories)
                    {
                        _dbContext.Set<ProductCategory>().Add(category);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            if (_dbContext.Products.Count() == 0)
            {
                var productData = File.ReadAllText("../MangaRestaurant.Repository/Data/DataSeed/products.json");
                var products = JsonSerializer.Deserialize<List<Product>>(productData);

                if (products?.Count() > 0)
                {
                    ////الحل ده بنمسح الاى دى علشان ميضربش ايرور او نمسحه من الملف احسن
                    //products = products.Select(b => new ProductBrand()
                    //{
                    //    Name = b.Name,

                    //}).ToList();

                    foreach (var product in products)
                    {
                        _dbContext.Set<Product>().Add(product);
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }
            if (!_dbContext.Set<DeliveryMethod>().Any(m => m.ShortName == "Dine-In"))
            {
                var dineIn = new DeliveryMethod
                {
                    ShortName = "Dine-In",
                    Description = "Eat inside the restaurant",
                    DeliveryTime = "Immediate",
                    Cost = 0
                };
                _dbContext.Set<DeliveryMethod>().Add(dineIn);
                await _dbContext.SaveChangesAsync();
            }

            if (!_dbContext.Set<DeliveryMethod>().Any())
            {
                var deliveryData = File.ReadAllText("../MangaRestaurant.Repository/Data/DataSeed/delivery.json");
                var deliveries = JsonSerializer.Deserialize<List<DeliveryMethod>>(deliveryData);

                if (deliveries?.Count() > 0)
                {
                    foreach (var delivery in deliveries)
                    {
                        if (!_dbContext.Set<DeliveryMethod>().Any(m => m.ShortName == delivery.ShortName))
                        {
                            _dbContext.Set<DeliveryMethod>().Add(delivery);
                        }
                    }
                    await _dbContext.SaveChangesAsync();
                }
            }

            if (!_dbContext.SiteSettings.Any())
            {
                var settings = new SiteSettings
                {
                    RestaurantName = "Manga Restaurant",
                    RestaurantNameAr = "مطعم مانجا",
                    Address = "123 Manga Street, Cairo, Egypt",
                    AddressAr = "123 شارع مانجا، القاهرة، مصر",
                    Phone1 = "+20 123 456 7890",
                    Phone2 = "+20 100 123 4567",
                    Email = "info@mangarestaurant.com",
                    CurrencyCode = "EGP",
                    CurrencySymbol = "ج.م",
                    FacebookUrl = "https://facebook.com/mangarestaurant",
                    InstagramUrl = "https://instagram.com/mangarestaurant",
                    TwitterUrl = "https://twitter.com/mangarestaurant",
                    OpeningHoursEn = "Mon-Sun: 10:00 AM - 11:00 PM",
                    OpeningHoursAr = "الإثنين-الأحد: 10:00 صباحاً - 11:00 مساءً",
                    DeliveryFee = 25.0m
                };

                _dbContext.SiteSettings.Add(settings);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
