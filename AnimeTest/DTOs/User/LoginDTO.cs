using System;
using System.ComponentModel.DataAnnotations;

namespace AnimeBack.DTOs.User
{
    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
