using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeBack.DTOs;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using AnimeBack.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AnimeBack.Controllers
{
    [ApiController]

    [Route("api/[controller]")]
    public class CartController : BaseController
    {
        private readonly DataContext _context;

        public CartController(DataContext context)
        {
            _context = context;
        }

        [HttpGet(Name = "GetCart")]
        public async Task<ActionResult<CartDto>> GetCartAsync()
        {
            var cart = await GetCart(GetBuyerId());

            if (cart == null)
            {
                return NotFound();
            }
            return cart.MapCartDto();
        }



        [HttpPost]  //querystring -- api/basket?productId=3&quantity=2
        public async Task<ActionResult<CartDto>> AddToCart(int productId, int quantity = 1)
        {


            //get cart

            var cart = await GetCart(GetBuyerId());

            //create cart

            if (cart == null) cart = CreateCart();

            //get product

            var product = await _context.Products.FindAsync(productId);

            if (product == null) return BadRequest(new ProblemDetails { Title = "Product not found" });




            //add item
            cart.AddItem(product, quantity);
            //save changes

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return CreatedAtRoute("GetCart", cart.MapCartDto());
            //return StatusCode(201);

            return BadRequest(new ProblemDetails { Title = "problem saving item to cart" });


        }


        [HttpDelete]
        public async Task<ActionResult> DeleteFromCart(int productId, int quantity)
        {
            //get cart

            var cart = await GetCart(GetBuyerId());

            if (cart == null) return NotFound();

            //reduce quantity or remove item

            cart.RemoveItem(productId, quantity);
            //savechanges

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem deleting item from cart" });
        }


        //Methods

        


        private Cart CreateCart()
        {
            var buyerId = User.Identity?.Name;

            if (string.IsNullOrEmpty(buyerId))
            {
                buyerId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions {
                    Secure = true,
                    SameSite  = SameSiteMode.None,
                    IsEssential = true, Expires = DateTime.Now.AddDays(30),
                    
                 };

                Response.Cookies.Append("buyerId", buyerId, cookieOptions);
            }




            var cart = new Cart { BuyerId = buyerId };
            _context.Carts.Add(cart);

            return cart;
        }

        private async Task<Cart> GetCart(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }
            var cartExist = await _context.Carts
            .Include(i => i.Items)
            .ThenInclude(p => p.Product)
            .OrderByDescending(x => x.BuyerId.ToLower() == buyerId.ToLower()).FirstOrDefaultAsync();

            return cartExist;
        }

        private string GetBuyerId()
        {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }

    }
}