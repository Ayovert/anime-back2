using System;
using System.ComponentModel.DataAnnotations;

namespace AnimeBack.Entities
{
    public class Inventory
    {
        [Key]
        public int Id {get; set;}
        public int QuantityInStock {get; set;}
        public DateTime CreatedAt {get; set;}
        public DateTime ModifiedAt {get; set;}
        public DateTime DeletedAt {get; set;}

        public int ProductId {get; set;}
        public Product Product {get; set;}


    }
}