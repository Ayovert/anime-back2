using System.Collections.Generic;

namespace AnimeBack.DTOs
{
    public class CartDto
    {
        public int Id {get; set;}
        public string BuyerId {get; set;}

        public List<CartItemDto> Items {get; set;}

        public string PaymentIntentId {get; set;}

        public string ClientSecret {get; set;}

        public string TranRef {get; set;}

    }
}