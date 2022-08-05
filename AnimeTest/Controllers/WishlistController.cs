using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using Microsoft.AspNetCore.Authorization;
using AnimeBack.DTOs.WishList;
using AnimeBack.Extensions;

namespace AnimeBack.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : BaseController
    {
        private readonly DataContext _context;

        public WishlistController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Wishlist
        [HttpGet(Name = "GetWishList")]
        public async Task<ActionResult<WishListDTO>> GetWishListAsync()
        {
            var wishlists = await GetWishList();



            if (wishlists == null)
            {
                return NotFound();
            }
            return wishlists.MapWishListDto();

        }






        // POST: api/Wishlist
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<WishListDTO>> AddToWishList(int productId)
        {


            //get wishlist

            var wishlist = await GetWishList();

            //create wishlist

            if (wishlist == null)
            {
                wishlist = new WishList { UserId = User.Identity.Name };
                _context.WishLists.Add(wishlist);
            }


            //get product

            var product = await _context.Products.FindAsync(productId);

            if (product == null) return NotFound(new ProblemDetails { Title = "Product not found" });



            var productInWishlist = wishlist.Items.FindIndex(x => x.ProductId == productId);

            if (productInWishlist > -1)
            {
                return BadRequest(new ProblemDetails { Title = "product exists in wishlist already" });
            }


            //add item
            wishlist.AddItem(product);
            //save changes

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return CreatedAtRoute("GetWishList", wishlist.MapWishListDto());
            //return StatusCode(201);

            return BadRequest(new ProblemDetails { Title = "problem adding item to wishlist" });


        }

        // DELETE: api/Wishlist/5
        [HttpDelete]
        public async Task<ActionResult> DeleteFromWishList(int productId)
        {
            //get cart

            var wishList = await GetWishList();

            if (wishList == null) return NotFound();

          


            //reduce quantity or remove item



            wishList.RemoveItem(productId);
            //savechanges

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem deleting item from wishlist" });
        }

        private async Task<WishList> GetWishList()
        {
            var wishlist = await _context.WishLists.Include(i => i.Items)
             .ThenInclude(p => p.Product)
             .OrderByDescending(x => x.UserId == User.Identity.Name).FirstOrDefaultAsync();

            return wishlist;
        }
    }
}
