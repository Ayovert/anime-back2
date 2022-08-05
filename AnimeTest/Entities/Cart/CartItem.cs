using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeBack.Entities
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int Id {get; set;}

        public int Quantity {get; set;}
        public int ProductId{get; set;}
        public Product Product {get; set;}
        


        public int CartId {get; set;}

        public Cart Cart {get; set;}
       

        public DateTime CreatedAt {get; set;}  = DateTime.Now;
        public DateTime ModifiedAt {get; set;} = DateTime.Now;

    }
}