namespace AnimeBack.DTOs.TransactionModel
{
    public class WebhookRequest
    {
        public string id {get; set;}
        public string txRef {get; set;}
        public string flwRef {get; set;}

        public string orderRef {get; set;}

        public string amount {get; set;}
        public string charged_amount {get; set;}

        public string status {get; set;}
        public string currency {get; set; }

        public CustomerWeb customer {get; set;}

    }


    public class CustomerWeb
    {
        public string id {get; set;}
        public string fullName {get; set;}

        public string email {get; set;}
    }
}

/*{"id":3494259,"txRef":"FLW637910723663579184","flwRef":"FLW-MOCK-a17686f6bf1d34075b8ba091cb660f9c","orderRef":"URF_1655471988370_6836335","paymentPlan":null,"paymentPage":null,"createdAt":"2022-06-17T13:19:48.000Z","amount":338.63,"charged_amount":338.63,"status":"successful","IP":"52.209.154.143","currency":"NGN","appfee":4.74,"merchantfee":0,"merchantbearsfee":1,"customer":{"id":1662105,"phone":null,"fullName":"Ayo Address","customertoken":null,"email":"haryorbumz@gmail.com","createdAt":"2022-06-17T13:19:48.000Z","updatedAt":"2022-06-17T13:19:48.000Z","deletedAt":null,"AccountId":1773976},"entity":{"card6":"553188","card_last4":"2950","card_country_iso":"NG","createdAt":"2020-04-24T15:19:22.000Z"},"event.type":"CARD_TRANSACTION"}*/