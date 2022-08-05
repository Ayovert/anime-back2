using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AnimeBack.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly EmailConfiguration _emailConfig;
        private readonly IWebHostEnvironment _env;


        public EmailService(IConfiguration configuration, ILogger<EmailService> logger, EmailConfiguration emailConfig, IWebHostEnvironment env)
        {
            _configuration = configuration;
            _logger = logger;
            _emailConfig = emailConfig;
            _env = env;
        }

        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            Send(emailMessage);
        }

        public async Task SendEmailAsync(Message message)
        {
            var mailMessage = CreateEmailMessage(message);

            await SendAsync(mailMessage);
        }

        private MimeMessage CreateEmailMessage(Message message)
        {
             var emailMessage = new MimeMessage();
            try{
                
            emailMessage.From.Add(new MailboxAddress(message.From, message.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.MessageContent.Subject;

            var seperator = Path.DirectorySeparatorChar.ToString();

            var emailLogo = base64Image.EmailLogo;

          /*  var pathToFile = _env.ContentRootPath
            + seperator
            + "Services"
            + seperator
            + "EmailService"
            + seperator;*/

            var pathToFile = _env.WebRootPath + seperator;




            var emailHtmlPath = pathToFile + "emailtemplates"
            + seperator + message.MessageContent.EmailType + ".html";

            var bodyBuilder2 = new BodyBuilder();

            using (StreamReader SourceReader = System.IO.File.OpenText(emailHtmlPath))
            {
                bodyBuilder2.HtmlBody = SourceReader.ReadToEnd();
            }
            // 2. add a header image linked resource to the builder

            message.HeaderImage.ContentPath = pathToFile + string.Format( "{0}{1}{2}", "images",seperator,"clouds3.svg");


            var header = bodyBuilder2.LinkedResources.Add(
                message.HeaderImage.ContentPath);

            // 3. set the contentId for the resource 
            // added to the builder to the contentId passed
            header.ContentId = message.HeaderImage.ContentId;

            if(message.MessageContent.EmailType == "orders")
            {
                bodyBuilder2.HtmlBody = string.Format(
                bodyBuilder2.HtmlBody,
            message.MessageContent.Subject,
            message.Content,
            message.MessageContent.UserName,
            message.MessageContent.order.OrderId,
            message.MessageContent.order.OrderStatus,
            message.MessageContent.order.PaymentStatus,
            message.MessageContent.order.OrderDate,
            emailLogo
            );
            }else{
                bodyBuilder2.HtmlBody = string.Format(
                bodyBuilder2.HtmlBody,
            message.MessageContent.Subject,
            message.Content,
            message.MessageContent.UserName,
            emailLogo
            );
            }


            

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = string.Format("<h2 style='color:red;'>OTP</h2>"
            + "<h1>{0}</h1>"
            ,

            message.Content
            )
            };

            if (message.Attachments != null && message.Attachments.Any())
            {
                byte[] fileBytes;
                foreach (var attachment in message.Attachments)
                {
                    using (var ms = new MemoryStream())
                    {
                        attachment.CopyTo(ms);
                        fileBytes = ms.ToArray();
                    }

                    bodyBuilder2.Attachments.Add(attachment.FileName, fileBytes, ContentType.Parse(attachment.ContentType));
                }
            }

            emailMessage.Body = bodyBuilder2.ToMessageBody();

            }catch(Exception ex)
            {
                 _logger.LogError($"Error creating Email - \n \n {ex.Message}");
                    throw;

            }
           
            return emailMessage;
        }

        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_configuration["SmtpServer"], _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_configuration["UserName"], _configuration["Password"]);

                    client.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    //log an error message or throw an exception, or both.
                    _logger.LogError($"Error sending Email - \n \n {ex.Message}");
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }


        private async Task SendAsync(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(_emailConfig.SmtpServer, _emailConfig.Port, SecureSocketOptions.Auto);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    await client.AuthenticateAsync(_emailConfig.UserName, _emailConfig.Password);

                    await client.SendAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    //log an error message or throw an exception, or both.

                    _logger.LogError($"Error sending Email - \n \n {ex.Message}");
                    throw;
                }
                finally
                {
                    await client.DisconnectAsync(true);
                    client.Dispose();
                }
            }
        }
    }
}