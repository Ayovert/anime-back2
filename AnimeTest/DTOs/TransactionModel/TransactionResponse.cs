using Newtonsoft.Json;

namespace AnimeBack.DTOs.TransactionModel
{
    public class TransactionRequest
    {
        public int TransactionId { get; set; }
    }


    public class TransactionResponse
    {
        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("message")]
        public string message { get; set; }
        [JsonProperty("data")]
        public DataClass data { get; set; }
    }

    public class DataClass
    {

        public int id { set; get; }
        public string tx_ref { set; get; }
        public string flw_ref { get; set; }
        public double amount { set; get; }
        public string currency { set; get; }
        public double charged_amount { set; get; }
 
        public double app_fee { get; set; }

        public string status { get; set; }
        public string narration { get; set; }

        public string payment_type { get; set; }

        public string created_at { get; set; }

        //public string meta {get; set;}

        public double amount_settled { get; set; }

        public Customer customer { get; set; }
    }

    public class Customer
    {
        public int id { get; set; }
        public string email { get; set; }
        public string phone_number { get; set; }
        public string name { get; set; }
    }
}


