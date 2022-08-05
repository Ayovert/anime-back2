using System;
using System.Linq;
using AnimeBack.DTOs;
using AnimeBack.DTOs.Product;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeBack.Extensions
{
    public static class CartExtensions
    {
        
        public static CartDto MapCartDto( this Cart cart)
        {

            var utilMethods = new UtilMethods();
            return new CartDto
            {
                Id = cart.Id,
                BuyerId = cart.BuyerId,
                PaymentIntentId = cart.PaymentIntentId,
                ClientSecret = cart.ClientSecret,
                TranRef = cart.TranRef,
                Items = cart.Items.Select(item => new CartItemDto
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    Description = item.Product.Description,


                    Category = utilMethods.GetCategory(item.Product.CategoryId),


                    QuantityInStock =  item.Product.QuantityInStock,
                    PictureUrl = item.Product.PictureUrl,
                    Quantity = item.Quantity

                }).ToList()
            };
        }

        public static string getCategory(int categoryId)
        {
            //var category = _context.Categories.ToList();
            string category1 = "Misc";

            var category = Enum.GetName(typeof(CategoryModel), categoryId);



            

            if (category == null) return null;

            
                return category1;
           
        }


        public static IQueryable<Cart> GetCartwithItems(this IQueryable<Cart> query, string buyerId)
        {
            
           
            var cart =  query.Include(i => i.Items)
            .ThenInclude(p => p.Product)
            .OrderByDescending(x => x.BuyerId.ToLower() == buyerId.ToLower());


            return cart;
        }


    }
}