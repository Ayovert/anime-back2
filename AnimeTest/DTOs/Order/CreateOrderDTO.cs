using AnimeBack.Entities.OrderAggregate;

namespace AnimeBack.DTOs.Order
{
    public class CreateOrderDTO
    {
        public bool SaveAddress {get; set;}

      //  public string paymentStatus {get; set;}
        //public int transactionId {get; set;} 

        //public double chargedAmount {get; set;}

        

        public ShippingAddress ShippingAddress {get; set;}

        public string CheckoutUrl {get; set;}
    }


    public class UpdateOrderDTO
    {

        public string paymentStatus {get; set;}
        public int transactionId {get; set;} 

        public double chargedAmount {get; set;}

    }
}