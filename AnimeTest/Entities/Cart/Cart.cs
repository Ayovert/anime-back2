using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace AnimeBack.Entities
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        
        public string BuyerId { get; set; }

        //public int UserId {get; set;}
        //public User user {get; set;}
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public string PaymentIntentId {get; set;}
        public string ClientSecret {get; set;}

        public string TranRef {get; set;}
        
        public void AddItem(Product product, int quantity)
        {
            if (Items.All(item => item.ProductId != product.Id))
            {
                Items.Add(new CartItem { Product = product, Quantity = quantity });
            }

            var existingItem = Items.FirstOrDefault(item => item.ProductId == product.Id);

        
       
            if (existingItem != null){ 
                existingItem.Quantity += quantity;
                existingItem.ModifiedAt = DateTime.Now;
            }

        
        }

        public void RemoveItem(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(item => item.ProductId == productId);

            if (item == null) return;
            item.Quantity -= quantity;

            if (item.Quantity <= 0)
            {
                Items.Remove(item);
            }
        }
    }
}