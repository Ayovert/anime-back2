using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeBack.Entities;
using AnimeBack.Entities.PaymentAggregate;
using Microsoft.Extensions.Configuration;
using Stripe;
using AnimeBack.Helpers;
using Microsoft.AspNetCore.Mvc;
using AnimeBack.DTOs.Response;
using AnimeBack.DTOs.Order;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using AnimeBack.Middleware;

namespace AnimeBack.Services
{
    public class PaymentService
    {
        private readonly IConfiguration _config;
        private readonly DataContext _context;

         private readonly DataClient _client;

         private readonly ILogger<ExceptionMiddleware> _logger;
        public PaymentService(IConfiguration config, DataContext context, DataClient client, ILogger<ExceptionMiddleware> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _context = context ?? throw new ArgumentNullException(nameof(context));

            _client = client ?? throw new ArgumentNullException(nameof(client));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<PaymentIntent> CreateorUpdatePaymentIntent(Cart cart)
        {
            StripeConfiguration.ApiKey = _config["StripeSettings:SecretKey"];
            var intent = new PaymentIntent();

            var service = new PaymentIntentService();

            var subtotal = cart.Items.Sum(item => item.Quantity * item.Product.Price);
            var deliveryFee = subtotal > 10000 ? 0 : 500;

            if (String.IsNullOrEmpty(cart.PaymentIntentId))
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = subtotal + deliveryFee,
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" }
                };

                intent = await service.CreateAsync(options);

            }
            else
            {
                var options = new PaymentIntentUpdateOptions
                {
                    Amount = subtotal + deliveryFee
                };

                intent = await service.UpdateAsync(cart.PaymentIntentId, options);
            }

            return intent;



        }

        public async Task<CreateFlutterResponse> CreateFlutterPayment(CreateFlutterRequest flutterRequest)
        {

            /*const response = await got.post("https://api.flutterwave.com/v3/payments", {
        headers: {
            Authorization: `Bearer ${process.env.FLW_SECRET_KEY}`
        },
        json: {
            tx_ref: "hooli-tx-1920bbtytty",
            amount: "100",
            currency: "NGN",
            redirect_url: "https://webhook.site/9d0b00ba-9a69-44fa-a43d-a82c33c36fdc",
            meta: {
                consumer_id: 23,
                consumer_mac: "92a3-912ba-1192a"
            },
            customer: {
                email: "user@gmail.com",
                phonenumber: "080****4528",
                name: "Yemi Desola"
            },
            customizations: {
                title: "Pied Piper Payments",
                logo: "http://www.piedpiper.com/app/themes/joystick-v27/images/logo.png"
            }
        }
    }).json();*/


    Console.WriteLine(flutterRequest);

    var responseObj = new GenericResponse{};

    var url = $"https://api.flutterwave.com/v3/payments";
            Console.WriteLine(url);
            var responseStr = await _client.Post(flutterRequest, url);

            Console.WriteLine(responseStr);
            //return  response;
            var response = JsonConvert.DeserializeObject<CreateFlutterResponse>(responseStr);

            Console.WriteLine(response);

            if(response.status == "success"){

                _logger.LogInformation(response.message);

                return response;


            }

            return null;


            //  var order = await _context.Orders.FindAsync(orderId);

            //   if (order == null) return null


        }


        public async Task<GenericResponse> UpdateFlutterPayment(int orderId, string tranRef, string status, int TransactionId, double chargedAmount, int userId)
        {


            //  var order = await _context.Orders.FindAsync(orderId);

            //   if (order == null) return null;

            var paymentDetail = _context.PaymentDetails
            .FirstOrDefault(x => x.TransactionRef == tranRef);

            if (paymentDetail != null)
            {
                if (status == "completed")
                {
                    status = "successful";
                }
                else
                {
                    status = "cancelled";
                }

                PaymentStatus paymentStatus;

                var statusExists = Enum.TryParse(status, out paymentStatus);



                paymentDetail.OrderId = orderId;
                paymentDetail.Status = statusExists ? paymentStatus : PaymentStatus.failed;
                paymentDetail.TransactionId = TransactionId;
                paymentDetail.ModifiedAt = DateTime.Now;


                //  _context.Update(paymentDetail);



                var result = await _context.SaveChangesAsync() > 0;

                if (result)
                {
                    return new GenericResponse{responseCode= 200, responseMessage = "payment history updated"};
                }
            }



            return null;


        }



    }
}
