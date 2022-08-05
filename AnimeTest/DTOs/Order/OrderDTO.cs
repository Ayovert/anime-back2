using System;
using System.Collections.Generic;
using AnimeBack.Entities.OrderAggregate;

namespace AnimeBack.DTOs.Order
{
    public class OrderDTO
    {
        public int Id {get; set;}

        public string BuyerId {get; set;}
        public ShippingAddress ShippingAddress {get; set;}
        public DateTime OrderDate {get; set;}
        public List<OrderItemDTO> OrderItems {get; set;} 
        public long subtotal {get; set;}
        public long DeliveryFee {get; set;}
        public string OrderStatus {get;set;}

        public string TranRef {get; set;}

        public double Total {get; set;}
    }
}