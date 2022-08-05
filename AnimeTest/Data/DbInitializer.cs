using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AnimeBack.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeProduct(DataContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            if (!userManager.Users.Any())
            {

                var user = new User
                {
                    UserName = "Ayo",
                    Email = configuration["Email1"]
                };
                await userManager.CreateAsync(user, configuration["PasswordX"]);
                await userManager.AddToRoleAsync(user, "Member");


                var admin = new User
                {
                    UserName = "admin",
                    Email = configuration["Email2"]
                };
                await userManager.CreateAsync(admin, configuration["PasswordX"]);
                await userManager.AddToRolesAsync(admin, new[] { "Member", "Admin" });



            }



            if (context.Products.Any()) return;

            var products = new List<Product>
            {
                new Product
                {
                    Price = 100,
                    Name = "Kisame bag",
                    Description= "Kisame bag",
                    PictureUrl= "/images/products/bag1.png",
                    CategoryId= 4,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100

                },
                new Product
                {
                    Name = "Naruto T-Shirt",
                    Description =
                        "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 20000,
                    PictureUrl = "/images/products/narutoShirt.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Akatsuki Shirt",
                    Description = "Nunc viverra imperdiet enim. Fusce est. Vivamus a tellus.",
                    Price = 20000,
                    PictureUrl = "/images/products/akatsukiShirt.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Ankle Socks",
                    Description =
                        "Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.",
                    Price = 20000,
                    PictureUrl = "/images/products/ankleSocks.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Boku no hero Bag",
                    Description =
                        "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Proin pharetra nonummy pede. Mauris et orci.",
                    Price = 30000,
                    PictureUrl = "/images/products/bag1.png",
                    CategoryId = 4,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Luffy Cap",
                    Description =
                        "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 25000,
                    PictureUrl = "/images/products/cap1.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                   QuantityInStock = 100
                },
                new Product
                {
                    Name = "Fish Accessory",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1000,
                    PictureUrl = "/images/products/fish1.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                 new Product
                {
                    Name = "Night light",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 8000,
                    PictureUrl = "/images/products/globe1.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Demon Slayer Necklace",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1500,
                    PictureUrl = "/images/products/necklace.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "One Piece Ring ",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1800,
                    PictureUrl = "/images/products/ring1.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Tanjiro X Nezuko Wallpaper",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1500,
                    PictureUrl = "/images/products/tanjiro1.png",
                    CategoryId = 2,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Ichigo Hollow T-Shirt",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1600,
                    PictureUrl = "/images/products/ichigo-hollow.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Bleach White T-Shirt",
                    Description =
                        "Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 1400,
                    PictureUrl = "/images/products/bleach_white.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Akatsuki Hoodie",
                    Description =
                        "Suspendisse dui purus, scelerisque at, vulputate vitae, pretium mattis, nunc. Mauris eget neque at sem venenatis eleifend. Ut nonummy.",
                    Price = 25000,
                    PictureUrl = "/images/products/akatsukiHoodie.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                },
                new Product
                {
                    Name = "Girl T-Shirt",
                    Description =
                        "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere, magna sed pulvinar ultricies, purus lectus malesuada libero, sit amet commodo magna eros quis urna.",
                    Price = 18999,
                    PictureUrl = "/images/products/girl_tee.png",
                    CategoryId = 1,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now,
                    DeletedAt =  new DateTime(),
                    QuantityInStock = 100
                }
            };



            foreach (var product in products)
            {
                context.Products.Add(product);
            }




            context.SaveChanges();
        }


    }
}