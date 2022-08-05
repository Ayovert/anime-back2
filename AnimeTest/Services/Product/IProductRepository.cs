using System.Collections.Generic;
using AnimeBack.Entities;

namespace AnimeBack.Services
{
    public interface IProductRepository
    {
         Product GetProduct(int Id);

         IEnumerable<Product> GetAllProducts();
    }
}