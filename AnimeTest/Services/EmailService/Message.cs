using System;
using System.Collections.Generic;
using System.Linq;
using AnimeBack.DTOs.Order;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MimeKit;

namespace AnimeBack.Services.EmailService
{
    public class Message
    {
        public List<MailboxAddress> To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string From { get; set; }

        
        public LinkedResource HeaderImage { get; set; }
        public MessageContent MessageContent {get; set;}
        public IFormFileCollection Attachments { get; set; }

        public Message(IEnumerable<string> to, string from, string content, MessageContent messageContent, IFormFileCollection attachments)
        {
            MessageContent = messageContent;

            HeaderImage = new LinkedResource{
                ContentId= "logo",
                ContentType="image/svg"
            };

            From = from;
            To = new List<MailboxAddress>();
            To.AddRange(to.Select(x => new MailboxAddress(x, x)));
            Content = content;
            Attachments = attachments;
        }



    }

    public class MessageContent
    {
        public string Subject {get; set;}
        public string Token {get; set;}
        public string UserName{get; set;}
        public string EmailType {get; set;}
        public OrderTracking order {get; set;}
    }

    public class OrderTracking 
    {
        public int OrderId {get; set;}

        public string OrderStatus {get; set;}

        public string PaymentStatus {get; set;}

        public DateTime OrderDate {get; set;}
    }

    public class MailContentDTO
    {
        public LinkedResource HeaderImage { get; set; }
        public string HtmlContent { get; set; }
        public LinkedResource FooterImage { get; set; }
        public LinkedResource Attachment { get; set; }
    }

    public class LinkedResource
    {
        public string ContentId { get; set; }
        public string ContentPath { get; set; }
        public string ContentType { get; set; }
        public byte[] ContentBytes { get; set; }
    }
}