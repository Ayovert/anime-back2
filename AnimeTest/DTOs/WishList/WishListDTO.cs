using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeBack.DTOs.WishList
{
    public class WishListDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public List<WishListItemDTO> Items { get; set; }

      
    }
}
