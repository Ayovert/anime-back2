using System.ComponentModel.DataAnnotations;

namespace AnimeBack.Entities
{
    public class UserAddress : Address
    {
        [Key]
        public int Id {get; set;}
     
    }

}