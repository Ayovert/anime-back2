using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeBack.Entities
{
    public class Product
    {
        [Key]
        public int Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public string SKU {get; set;}

        [ForeignKey("Category")]
        public int CategoryId {get; set;}

        public long Price {get;set;}

        [ForeignKey("Discount")]
        public int DiscountId {get; set;}

        
        public string PictureUrl {get; set;}

        public int QuantityInStock {get; set;}

        public string PublicId {get; set;}
        public DateTime CreatedAt {get; set;} = DateTime.Now;
        public DateTime ModifiedAt {get; set;} = DateTime.Now;
        public DateTime? DeletedAt {get; set;}
        
        

    }
}