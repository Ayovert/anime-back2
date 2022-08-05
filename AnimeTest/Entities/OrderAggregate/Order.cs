using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using AnimeBack.Entities.PaymentAggregate;

namespace AnimeBack.Entities.OrderAggregate
{
    public class Order
    {
        public int Id {get; set;}

        public string BuyerId {get; set;}
        public ShippingAddress ShippingAddress {get; set;}
        public DateTime OrderDate {get; set;} = DateTime.Now;
        public List<OrderItem> OrderItems {get; set;} 
        public long Subtotal {get; set;}
        public long DeliveryFee {get; set;}

        public string PaymentIntentId {get; set;}
        public OrderStatus OrderStatus {get;set;}

        public string TranRef {get; set;}

        public long GetTotal() => Subtotal + DeliveryFee;
    }
}