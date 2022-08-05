using AnimeBack.DTOs.WishList;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeBack.Extensions
{
    public static class WishListExtension
    {

        public static WishListDTO MapWishListDto(this WishList wish)
        {
            var utilMethods = new UtilMethods();

            return new WishListDTO
            {
                Id = wish.Id,
                UserId = wish.UserId,
                
                Items = wish.Items.Select(item => new WishListItemDTO
                {
                    ProductId = item.ProductId,
                    Name = item.Product.Name,
                    Price = item.Product.Price,
                    Description = item.Product.Description,


                    Category = utilMethods.GetCategory(item.Product.CategoryId),

                    PictureUrl = item.Product.PictureUrl

                }).ToList()
            };
        }
    }
}
