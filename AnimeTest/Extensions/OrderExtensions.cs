using System.Linq;
using AnimeBack.DTOs.Order;
using AnimeBack.Entities.OrderAggregate;
using AnimeBack.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AnimeBack.Extensions
{
    public static class OrderExtensions
    {
        public static IQueryable<OrderDTO> MapOrdertoOrderDTO(this IQueryable<Order> query)
        {
            var util = new UtilMethods();
            var orderDto = query.Select(order => new OrderDTO
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                DeliveryFee = order.DeliveryFee,
                subtotal = order.Subtotal,
                OrderStatus = util.GetStatusString((int)order.OrderStatus),
                Total = order.GetTotal(),
                TranRef = order.TranRef,
                OrderItems = order.OrderItems.Select(item => new OrderItemDTO
                {
                    ProductId = item.ItemOrdered.ProductId,
                    Name = item.ItemOrdered.Name,
                    PictureUrl = item.ItemOrdered.PictureUrl,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            }).AsNoTracking();

            return orderDto;
        }

    }



}


//PaymentInitiatedOrderNotProcesssed,
//      PaymentFailedOrderNotProcessed,
//    PaymentSuccesfullOrderProccesed