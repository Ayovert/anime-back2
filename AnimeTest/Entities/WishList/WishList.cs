
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeBack.Entities
{
    public class WishList
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        //public int UserId {get; set;}
        //public User user {get; set;}
        public List<WishListItem> Items { get; set; } = new List<WishListItem>();

        public void AddItem(Product product)
        {
            if (Items.All(item => item.ProductId != product.Id))
            {
                Items.Add(new WishListItem { Product = product});
            }
        }

        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(item => item.ProductId == productId);

            if (item == null) return;
            
                Items.Remove(item);
        }
    }
}
