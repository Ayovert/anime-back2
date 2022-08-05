using System;
using System.ComponentModel.DataAnnotations;

namespace AnimeBack.Entities
{
    public class Category
    {
        [Key]
        public int Id {get; set;}
        public string Name {get; set;}
        public string Description {get; set;}
        public DateTime CreatedAt {get; set;}
        public DateTime ModifiedAt {get; set;}
        public DateTime DeletedAt {get; set;}
        

    }
}