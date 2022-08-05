using AnimeBack.DTOs.Product;
using AnimeBack.Entities;
using AutoMapper;

namespace AnimeBack.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();
        }
    }
}