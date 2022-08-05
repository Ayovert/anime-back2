using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnimeBack.DTOs;
using AnimeBack.DTOs.Product;
using AnimeBack.Entities;
using AnimeBack.Extensions;
using AnimeBack.Helpers;
using AnimeBack.Middleware;
using AnimeBack.RequestHelpers;
using AnimeBack.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnimeBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : BaseController
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ImageService _imageService;


        private readonly ILogger<ExceptionMiddleware> _logger;

        public ProductsController(DataContext context, IMapper mapper, ImageService imageService, ILogger<ExceptionMiddleware> logger)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _logger = logger;
        }


        [HttpGet]
        public async Task<ActionResult<List<ProductDTO>>> GetProducts([FromQuery] ProductParams productParams)
        {
            var query = _context.Products
            .Sort(productParams.OrderBy)
            .Search(productParams.searchTerm)
            .Filter(productParams.Categories)
            .AsQueryable();

            







            //   var products = await query.ToListAsync();

            var productsList = await PagedList<Product>.ToPagedList(query, productParams.PageNumber, productParams.PageSize);

            Response.AddPaginationHeader(productsList.MetaData);

            //var products = _context.Products
            // .ToList();

            var products = new List<ProductDTO>();

            foreach (var product in productsList)
            {
                var prod = MapProductDTO(product);

                products.Add(prod);

            }




            //productsDto = MapProductsDTO(products);
            return Ok(products);
        }




        [HttpGet("{Id}", Name = "GetProduct")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int Id)
        {
            var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == Id);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = MapProductDTO(product);

            return Ok(productDto);
        }

        [HttpGet("filters")]

        public async Task<IActionResult> GetFilters()
        {
            var categoryList = await _context.Products.Select(p => p.CategoryId).Distinct().ToListAsync();

            var categories = new List<string>();

            foreach (var category in categoryList)
            {
                CategoryModel category1;

                var catId = category.ToString();

                var catExists = Enum.TryParse(catId, true, out category1) && Enum.IsDefined(typeof(CategoryModel), category1);

                var catStr = category1.ToString();

                if (!catExists)
                {
                    catStr = "Misc";
                }


                categories.Add(catStr);

            }

            return Ok(new { categories });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<ActionResult<Product>> CreateProduct([FromForm] CreateProductDTO productDto)
        {

            var product = _mapper.Map<Product>(productDto);

            if(productDto.File != null)
            {
                var imageResult = await _imageService.AddImageAsync(productDto.File);

                if(imageResult.Error != null)
                {
                    return BadRequest(new ProblemDetails {Title = imageResult.Error.Message});
                }

                product.PictureUrl = imageResult.SecureUrl.ToString();
                product.PublicId = imageResult.PublicId;


            }

            _context.Products.Add(product);


            var result = await _context.SaveChangesAsync() > 0;
            if (result) return CreatedAtRoute("GetProduct", new { Id = product.Id }, product);

            return BadRequest(new ProblemDetails { Title = "Problem creating new product" });
        }


        [Authorize(Roles = "Admin")]
        [HttpPut]

        public async Task<ActionResult<Product>> UpdateProduct([FromForm] UpdateProductDTO productDto)
        {

            var product = await _context.Products.FindAsync(productDto.Id);

            if (product == null) return NotFound();

            _mapper.Map(productDto, product);

            if(productDto.File != null)
            {
                var imageResult = await _imageService.AddImageAsync(productDto.File);
                if(imageResult.Error != null)
                {
                    return BadRequest(new ProblemDetails {Title = imageResult.Error.Message});
                }

                if(!string.IsNullOrEmpty(product.PublicId))
                {
                    await _imageService.DeleteImageAsync(product.PublicId);
                }

                product.PictureUrl = imageResult.SecureUrl.ToString();
                product.PublicId = imageResult.PublicId;
            }

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok(product);

            return BadRequest(new ProblemDetails { Title = "Problem updating product" });
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{Id}")]

        public async Task<ActionResult<Product>> DeleteProduct(int Id)
        {
            var product = await _context.Products.FindAsync(Id);

            if (product == null) return NotFound();


            if(!string.IsNullOrEmpty(product.PublicId))
                {
                    try
                    {
                        await _imageService.DeleteImageAsync(product.PublicId);
                    }catch(Exception ex)
                    {
                        _logger.LogError($"error deleting image {product.PublicId} + \n {ex.Message} ");
                    }
                    
                }

            _context.Products.Remove(product);

            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();


            return BadRequest(new ProblemDetails { Title = "Problem deleting product" });

        }






        private ProductDTO MapProductDTO(Product product)
        {
            //var prods = new List<ProductDTO>();


            string category1 = "Misc";


            var category = Enum.GetName(typeof(CategoryModel), product.CategoryId);

            var prod = new ProductDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Category = category != null ? category : category1,
                QuantityInStock = product.QuantityInStock,
                Price = product.Price,
                PictureUrl = product.PictureUrl,
                DiscountId = product.DiscountId
            };





            return prod;
        }
    }
}