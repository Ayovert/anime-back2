using AnimeBack.Entities.OrderAggregate;

namespace AnimeBack.DTOs.Order
{
    public class CreateFlutterRequest
    {
        public string public_key {get; set;}
        public string tx_ref {get; set;}

        public double amount {get; set;}
        public string currency {get; set;}
        public string payment_options {get; set;}
        public string redirect_url {get; set;}

        public Meta meta {get; set;}

        public Customer customer {get; set;}

        public Customizations customizations {get; set;}
    }

    public class Customizations
    {
        public string title {get; set;}
        public string description {get; set;}
        public string logo {get; set;}
    }

    public class Customer
    {
        public string email {get; set;}
        public string phonenumber {get; set;}
        public string name {get; set;}


    }

    public class Meta
    {
        public string consumer_id {get; set;}
        public string consumer_mac {get; set;}
    }



     public class CreateFlutterResponse
    {
        public string status {get; set;}
        public string message {get; set;}

        public Data data {get; set;}
    }

    public class Data
    {
      public string link {get; set;}
    }
}


