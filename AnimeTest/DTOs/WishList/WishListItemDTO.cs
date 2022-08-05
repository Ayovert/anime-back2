using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeBack.DTOs.WishList
{
    public class WishListItemDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public long Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public string PictureUrl { get; set; }
    }
}
