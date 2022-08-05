using System;
using System.IO;
using System.Threading.Tasks;
using AnimeBack.DTOs.Response;
using AnimeBack.DTOs;
using AnimeBack.DTOs.Order;
using AnimeBack.DTOs.TransactionModel;
using AnimeBack.Entities;
using AnimeBack.Entities.OrderAggregate;
using AnimeBack.Entities.PaymentAggregate;
using AnimeBack.Extensions;
using AnimeBack.Helpers;
using AnimeBack.Middleware;
using AnimeBack.Services;
using AnimeBack.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stripe;

namespace AnimeBack.Controllers
{
    public class PaymentsController : BaseController
    {
        private readonly DataClient _client;


        private readonly PaymentService _paymentService;
        private readonly DataContext _context;
        private readonly IConfiguration _config;

        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IEmailService _emailService;

        public PaymentsController(PaymentService paymentService, DataContext context, IConfiguration config, DataClient client, ILogger<ExceptionMiddleware> logger, IEmailService emailService)
        {
            _config = config;
            _context = context;
            _paymentService = paymentService;
            _client = client;
            _logger = logger;
            _emailService = emailService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CartDto>> CreateorUpdatePaymentIntent()
        {

            var cart = await _context.Carts.GetCartwithItems(User.Identity.Name)
            .FirstOrDefaultAsync();

            if (cart == null) return NotFound();

            var intent = await _paymentService.CreateorUpdatePaymentIntent(cart);

            if (intent == null) return BadRequest(new ProblemDetails { Title = "Problem creating payment intent" });

            cart.PaymentIntentId = cart.PaymentIntentId ?? intent.Id;

            cart.ClientSecret = cart.ClientSecret ?? intent.ClientSecret;

            _context.Update(cart);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest(new ProblemDetails { Title = "Problem updating Cart with Intent" });


            return cart.MapCartDto();

        }




        [HttpPost("stripe_webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"],
                _config["StripeSettings:WhSecret"]);

            var charge = (Charge)stripeEvent.Data.Object;

            var order = await _context.Orders.FirstOrDefaultAsync(x =>
                x.PaymentIntentId == charge.PaymentIntentId);

            if (charge.Status == "succeeded") order.OrderStatus = OrderStatus.PaymentSuccesfullOrderProccesed;

            await _context.SaveChangesAsync();

            return new EmptyResult();
        }


        [HttpPost("webhook")]
        public async Task<ActionResult> FlutterWebhook()
        {

            var secretHash = _config["FlutterKeys:SecretHash"];

            var verifHash = Request.Headers["verif-hash"];
            if (string.IsNullOrEmpty(verifHash) || (verifHash != secretHash))
            {
                return Unauthorized(new ProblemDetails { Title = "webhook not from flutterwave" });
            }
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();


         

           
            _logger.LogInformation($"webhook payload : {json}");

           

            var response = JsonConvert.DeserializeObject<WebhookRequest>(json);

            var tranIdnum = Int32.TryParse(response.id, out var tranId);

            var verifyTran = await VerifyFlutterTransaction(tranId);

            if (verifyTran.responseCode == 200)
            {
                _logger.LogInformation("Transaction Verified Successfully");

            }
            else
            {
                _logger.LogError(verifyTran.responseMessage);
            }


            return Ok(response);
        }

        



        [HttpPost("verifyTran")]
        public async Task<GenericResponse> VerifyFlutterTransaction(int tranId)
        {
            var requestModel = new TransactionRequest();

            var responseObj = new GenericResponse();
            try
            {
                var url = $"https://api.flutterwave.com/v3/transactions/{tranId}/verify";
                Console.WriteLine(url);
                requestModel.TransactionId = tranId;
                var responseStr = await _client.Get(url);
                Console.WriteLine(responseStr);
                //return  response;
                var response = JsonConvert.DeserializeObject<TransactionResponse>(responseStr);


                if (response.status == "success")
                {

                    _logger.LogInformation(response.message);

                    var order = await _context.Orders.FirstOrDefaultAsync(x =>
                    x.TranRef == response.data.tx_ref);

                    var paymentDetail = await _context.PaymentDetails
                .FirstOrDefaultAsync(x => x.TransactionRef == response.data.tx_ref);

                    var user = await _context.Users
                    .FirstOrDefaultAsync(x => x.Email == response.data.customer.email);

                    if (order == null || paymentDetail == null || user == null)
                    {
                        responseObj.responseCode = 400;
                        responseObj.responseMessage = $"order not found";
                    }

                    if (response.data.status == paymentDetail.Status.ToString())
                    {
                        responseObj.responseCode = 400;
                        responseObj.responseMessage = $"Transaction Status has not changed";
                    }




                    if (response.data.status == "successful" && response.data.currency == paymentDetail.Currency && response.data.amount == paymentDetail.Amount)
                    {

                        order.OrderStatus = OrderStatus.PaymentSuccesfullOrderProccesed;

                        paymentDetail.Status = PaymentStatus.successful;

                    }
                    else
                    {
                        order.OrderStatus = OrderStatus.PaymentFailedOrderNotProcessed;

                        paymentDetail.Status = PaymentStatus.failed;

                    }

                    paymentDetail.AmountSettled = response.data.amount_settled;
                    paymentDetail.TransactionId = response.data.id;
                    paymentDetail.FlutterwaveRef = response.data.flw_ref;
                    paymentDetail.ModifiedAt = DateTime.Now;

                    var result = await _context.SaveChangesAsync() > 0;

                    if (result)
                    {
                        _logger.LogInformation($"{response.message} + and Records updated successfully");

                        var orderStatus = new UtilMethods().GetStatusString((int)order.OrderStatus);

                        var orderTracking = new OrderTracking
                        {
                            OrderId = order.Id,
                            OrderDate = order.OrderDate,
                            OrderStatus = orderStatus,
                            PaymentStatus = paymentDetail.Status.ToString()

                        };
                        SendOrderConfirmation(user, "Order Confirmation", "orders", orderTracking);
                        responseObj.responseCode = 200;
                        responseObj.responseMessage = $"{response.message} + and Records updated successfully";

                    }
                }
                if (response.status == "error")
                {
                    _logger.LogError(response.message);

                    responseObj.responseCode = 400;
                    responseObj.responseMessage = response.message;
                }




            }
            catch (Exception ex)
            {
                responseObj.responseCode = 500;
                responseObj.responseMessage = ex.Message;
            }

            return responseObj;
        }



        [Authorize]

        [HttpPost("flutter")]
        public async Task<ActionResult<CartDto>> CreateFlutterPayment()
        {
            var cart = await _context.Carts.GetCartwithItems(User.Identity.Name)
               .FirstOrDefaultAsync();

            if (cart == null) return NotFound(new ProblemDetails { Title = "Cart not found" });
            var tranRef = $"FLW{GenerateTranRef()}";

            var payment = new PaymentDetail
            {
                Provider = "FlutterWave",
                Status = PaymentStatus.initiated,
                TransactionRef = tranRef,
                CreatedAt = DateTime.Now
            };





            //_context.Orders.Add(order);


            _context.PaymentDetails.Add(payment);

            cart.TranRef = tranRef;

            //_context.Update(cart);



            var result = await _context.SaveChangesAsync() > 0;


            if (!result) return BadRequest(new ProblemDetails { Title = "Problem updating Cart with PaymentId" });

            return cart.MapCartDto();


        }


        private async void SendOrderConfirmation(User user, string subject, string action, OrderTracking order)
        {

            var frontUrl = _config["Url:frontEnd"];
           


           

            var orderId = order.OrderId.ToString();

            var messageContent = new MessageContent
            {
                Subject = subject,
                UserName = user.UserName,
                Token = orderId,
                EmailType = action,
                order = order
            };
            // var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);

           /* var hasOrigin = Request.Headers.TryGetValue("Origin", out var origin);

            if (hasOrigin)
            {
                callback = string.Format("{0}/{1}/{2}", frontUrl, action, orderId);
            }
            else
            {
                callback = Url.Action(action, "Order", new { orderId }, Request.Scheme);
            }*/

            var callback2 = Url.Action(action, "Order", new { orderId }, Request.Scheme);

            


             var callback = string.Format("{0}/{1}/{2}", frontUrl, action, orderId);

            var message = new Message(new string[] { user.Email }, "haryorbumz@gmail.com", callback, messageContent, null);


            await _emailService.SendEmailAsync(message);
        }

        private long GenerateTranRef()
        {
            return DateTime.Now.Ticks;
        }

        private string GetBuyerId()
        {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }


    }


}