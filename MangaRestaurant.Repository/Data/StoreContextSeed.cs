using MangaRestaurant.Core.Entities;
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
        }
    }
}
