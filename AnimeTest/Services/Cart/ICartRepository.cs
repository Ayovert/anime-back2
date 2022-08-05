using AnimeBack.Entities;

namespace AnimeBack.Services
{
    public interface ICartRepository
    {
         Cart GetCart();
         Cart CreateCart();

         void Delete();

    }
}