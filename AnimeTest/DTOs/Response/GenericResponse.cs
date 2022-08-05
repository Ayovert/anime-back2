using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AnimeBack.DTOs.Response
{
    public class GenericResponse
    {
       
    public int responseCode {get; set;}
    public string responseMessage {get; set;}

    }

  
}