using System;
using System.Collections.Generic;
using System.Linq;
using AnimeBack.DTOs.Product;
using AnimeBack.Entities;
using AnimeBack.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;

namespace AnimeBack.Extensions
{
    public static class ProductExtensions
    {
        public static IQueryable<Product> Sort( this IQueryable<Product> query, string orderBy)
        {
            if(string.IsNullOrWhiteSpace(orderBy)) 
            {
                return query.OrderBy(p => p.Name);
            }
            query = orderBy switch
            {
                "price" => query.OrderBy(p => p.Price),
                "priceDesc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            return query;
        }


        public static IQueryable<Product> Search(this IQueryable<Product> query , string searchTerm)
        {
            if(string.IsNullOrWhiteSpace(searchTerm)) 
            {
                return query;
            }

            var lowerSearchTerm = searchTerm.Trim().ToLower();

            return query.Where(p => p.Name.ToLower().Contains(lowerSearchTerm));

        }

        public static IQueryable<Product> Filter(this IQueryable<Product> query , string categories)
        {
          
            var categoryList = new List<string>();

            if(!string.IsNullOrEmpty(categories))
            {
                categoryList.AddRange(categories.ToLower().Split(",").ToList());
            }

            var categoryIdList = new List<int>();

            foreach(var category in categoryList)
            {
                CategoryModel category1;

                var statusExists= Enum.TryParse( category, true, out category1);
                int x = (int)category1;
                categoryIdList.Add(x);
            }

            query = query.Where(p => categoryIdList.Count == 0 || categoryIdList.Contains(p.CategoryId));

            return query;
        }

    }
}