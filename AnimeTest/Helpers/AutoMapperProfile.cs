using AnimeBack.DTOs;
using AnimeBack.DTOs.User;
using AnimeBack.Entities;
using AutoMapper;

namespace AnimeBack.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<RegisterDTO, User>();
            CreateMap<UpdateDTO, User>();
        }
    }
}
