using System;
using System.ComponentModel.DataAnnotations;

namespace AnimeBack.DTOs
{
    public class UserDTO
    {

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public string Email { get; set; }

       
        public string PhoneNumber { get; set; }

        
        public string Username { get; set; }

        public string Token {get; set;}

        public CartDto Cart { get; set;}

    }
}
