using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AnimeBack.Entities
{
    public class User : IdentityUser<int>
    {
        public UserAddress Address { get; set; }
    }

    public class UserMain
    {
        public int UserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string Username { get; set; }


        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

    }
}
