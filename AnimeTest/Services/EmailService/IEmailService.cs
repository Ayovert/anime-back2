using System.Threading.Tasks;

namespace AnimeBack.Services.EmailService
{
    public interface IEmailService
    {
         Task SendEmailAsync(Message message);
    }
}