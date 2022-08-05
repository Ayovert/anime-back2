using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AnimeBack.Entities.PaymentAggregate
{
    public class PaymentDetail
    {
        public int Id {get; set;}

        [ForeignKey("Order")]
        public int OrderId {get; set;}


        public int UserId {get; set;}

        public double Amount {get; set;}
        public double AmountSettled {get; set;}

        public string Currency {get; set;}
        public string Provider {get; set;}
        public PaymentStatus Status {get; set;}

        public string TransactionRef {get; set;}
        public string FlutterwaveRef {get; set;}
        public int TransactionId {get; set;}
        public DateTime CreatedAt {get; set;}

        public DateTime ModifiedAt {get; set;}

    }
}