using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AnimeBack.DTOs.User
{
    public class ExternalLoginModel
    {
       
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    public string UserName {get; set;}
    public UserLoginInfoModel info {get; set;}
    public ClaimsPrincipal Principal { get; set; }

    }

    public class UserLoginInfoModel{
        public string ProviderKey{get; set;}
        public string LoginProvider {get;set;}
        public string ProviderDisplayName {get; set;}

    }
}