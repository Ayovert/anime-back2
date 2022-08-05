namespace AnimeBack.RequestHelpers
{
    public class ProductParams : PaginationParams
    {
        public string OrderBy {get; set;}
        public string searchTerm {get; set;}
        public string Categories {get; set;}
    }
}