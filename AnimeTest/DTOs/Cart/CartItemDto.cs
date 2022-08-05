namespace AnimeBack.DTOs
{
    public class CartItemDto
    {
        public int ProductId {get; set;}
        public string Name {get; set;}
        public long Price {get;set;}
        public string Description {get; set;}
        public string Category {get; set;}

        public int QuantityInStock {get; set;}
        public string PictureUrl {get; set;}

        public int Quantity {get; set;}
    }
}