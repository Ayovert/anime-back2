using System.ComponentModel.DataAnnotations;

namespace AnimeBack.DTOs.User
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email {get; set;}
    }
}