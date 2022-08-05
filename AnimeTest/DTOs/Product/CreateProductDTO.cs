using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AnimeBack.DTOs.Product
{
    public class CreateProductDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }

        [Required]
        public IFormFile File { get; set; }

        [Required]
         public int CategoryId {get; set;}

         [Required]
         [Range(0, 200)]

        public int QuantityInStock {get; set;}

        [Required]
        [Range(100, Double.PositiveInfinity) ]
        public long Price { get; set; }

        public int DiscountId { get; set; }
        



   
        

      

        
        

        

    }
}