using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeBack.DTOs.Order;
using AnimeBack.Entities;
using AnimeBack.Entities.OrderAggregate;
using AnimeBack.Entities.PaymentAggregate;
using AnimeBack.Extensions;
using AnimeBack.Helpers;
using AnimeBack.Middleware;
using AnimeBack.Services;
using AnimeBack.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AnimeBack.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : BaseController
    {
        private readonly DataContext _context;
        private readonly PaymentService _paymentService;

        private readonly IEmailService _emailService;

        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IConfiguration _config;
        public OrdersController(DataContext context, PaymentService paymentService, IEmailService emailService, ILogger<ExceptionMiddleware> logger, IConfiguration config)
        {
            _context = context;
            _paymentService = paymentService;
            _emailService = emailService;
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderDTO>>> GetOrders()
        {
            var orders = await _context.Orders.MapOrdertoOrderDTO()
            .Where(x => x.BuyerId == User.Identity.Name)
            .ToListAsync();

            return orders;
        }

        [HttpGet("{id}", Name = "GetOrder")]

        public async Task<ActionResult<OrderDTO>> GetOrder(int id)
        {
            var order = await _context.Orders.MapOrdertoOrderDTO()
            .Where(x => x.BuyerId == User.Identity.Name && x.Id == id)
            .FirstOrDefaultAsync();

            return order;
        }

        [HttpGet("tranRef/{tranRef}", Name = "GetOrderByTxRef")]
        public async Task<ActionResult> GetOrderByTranRef(string tranRef)
        {
            var order = await _context.Orders.MapOrdertoOrderDTO()
            .Where(x => x.BuyerId == User.Identity.Name && x.TranRef == tranRef)
            .FirstOrDefaultAsync();

            if (order == null)
            {
                _logger.LogError("order not found");
                return NotFound("order not found");
            }

            return Ok(new { orderId = order.Id });
        }



       


        [HttpPost]

        public async Task<ActionResult<Order>> CreateOrder(CreateOrderDTO orderDTO)
        {

            var currency = "NGN";
            var cart = await _context.Carts.GetCartwithItems(User.Identity.Name).FirstOrDefaultAsync();

            var user = await _context.Users.Include(x => x.Address).FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

            if (cart == null) return BadRequest(new ProblemDetails { Title = " Could not locate Cart" });

            var items = new List<OrderItem>();

            foreach (var item in cart.Items)
            {
                var productItem = await _context.Products.FindAsync(item.ProductId);
                var itemOrdered = new ProductItemOrdered
                {
                    ProductId = productItem.Id,
                    Name = productItem.Name,
                    PictureUrl = productItem.PictureUrl
                };

                var orderItem = new OrderItem
                {
                    ItemOrdered = itemOrdered,
                    Price = productItem.Price,
                    Quantity = item.Quantity
                };
                items.Add(orderItem);
                productItem.QuantityInStock -= item.Quantity;

            }


            var subTotal = items.Sum(item => item.Price * item.Quantity);
            var deliveryFee = subTotal > 10000 ? 0 : 500;

            var tax = (7.5 / 100) * (subTotal + deliveryFee);

            var totalx = (subTotal + deliveryFee + tax) / 100;



            var totalNum = String.Format("{0:0.##}", totalx);

            Double.TryParse(totalNum, out double total);


            var tranRef = $"FW{GenerateTranRef()}";



            var order = new Order
            {
                OrderItems = items,
                BuyerId = User.Identity.Name,
                ShippingAddress = orderDTO.ShippingAddress,
                Subtotal = subTotal,
                DeliveryFee = deliveryFee,
                PaymentIntentId = cart.PaymentIntentId,
                TranRef = tranRef,
                OrderStatus = OrderStatus.PaymentInitiatedOrderNotProcesssed
            };









            var payment = new PaymentDetail
            {
                UserId = user.Id,
                Amount = total,
                Provider = "FlutterWave",
                Currency = currency,
                Status = PaymentStatus.initiated,
                TransactionRef = tranRef,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now

            };



            if (orderDTO.SaveAddress)
            {


                var address = new UserAddress
                {
                    FullName = orderDTO.ShippingAddress.FullName,
                    Address1 = orderDTO.ShippingAddress.Address1,
                    Address2 = orderDTO.ShippingAddress.Address2,
                    City = orderDTO.ShippingAddress.City,
                    State = orderDTO.ShippingAddress.State,
                    Country = orderDTO.ShippingAddress.Country,
                    PostalCode = orderDTO.ShippingAddress.PostalCode,
                    Telephone = orderDTO.ShippingAddress.Telephone,
                    Mobile = orderDTO.ShippingAddress.Mobile
                };

                user.Address = address;
                user.PhoneNumber = orderDTO.ShippingAddress.Telephone;
                // _context.Users.Update(user);


            }

            // await _paymentService.CreateFlutterPayment(order.TranRef, orderDTO.transactionId, user.Id);

            var paymentRequest = new CreateFlutterRequest
            {

                //   _config["JWTSettings:TokenKey"]
                public_key = _config["FlutterKeys:PublicKey"], //flutter
                tx_ref = tranRef, // cart
                amount = total, //cart total
                currency = currency,
                payment_options = "card, banktransfer, ussd", //state from radio cart page button
                redirect_url = orderDTO.CheckoutUrl, //location
                meta = new Meta
                {
                    consumer_id = user.Id.ToString(), //userId
                    consumer_mac = $"user+{tranRef}", //dunno
                },
                customer = new Customer
                {
                    email = user.Email, // user
                    phonenumber = orderDTO.ShippingAddress.Telephone, //user
                    name = orderDTO.ShippingAddress.FullName, // user
                },
                customizations = new Customizations
                {
                    title = "The Clans Store", // store name
                    description = "Payment for an anime merch",
                    logo = "https://www.logolynx.com/images/logolynx/22/2239ca38f5505fbfce7e55bbc0604386.jpeg", // logo
                }
            };



            _context.Orders.Add(order);

            _context.PaymentDetails.Add(payment);
            _context.Carts.Remove(cart);






            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {

                var createPayment = await _paymentService.CreateFlutterPayment(paymentRequest);


                if (createPayment == null)
                {
                    Console.WriteLine("could not create payment link");

                    _logger.LogError("could not create payment link");
                }




                //SendOrderConfirmation(user, order.Id.ToString(), "Order Confirmation", "orders");

                //create flutterwave request




                return CreatedAtRoute("GetOrder", new
                {
                    id = order.Id
                }, createPayment);
            }
            return BadRequest(new ProblemDetails { Title = "Order Creation could not be completed" });
        }




        private long GenerateTranRef()
        {
            return DateTime.Now.Ticks;
        }


    }
}


