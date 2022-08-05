using AnimeBack.DTOs;
using AnimeBack.DTOs.User;
using AnimeBack.Entities;
using AnimeBack.Extensions;
using AnimeBack.Helpers;
using AnimeBack.Middleware;
using AnimeBack.Services;
using AnimeBack.Services.EmailService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AnimeBack.Controllers
{
    public class AccountController : BaseController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        private readonly DataContext _context;
        private readonly ILogger<ExceptionMiddleware> _logger;

        private readonly IEmailService _emailService;

        private readonly IConfiguration _config;
        public AccountController(UserManager<User> userManager, TokenService tokenService, DataContext context, IEmailService emailService, SignInManager<User> signInManager, IConfiguration config,
            ILogger<ExceptionMiddleware> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginModel)
        {
            var user = await _userManager.FindByNameAsync(loginModel.Username);

            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(loginModel.Username);
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, loginModel.Password);
            if (user == null || !passwordCheck)
            {
                return Unauthorized(new ProblemDetails { Title = "Invalid Username or Password" });
            }


            var userCart = await GetCart(loginModel.Username);

            var anonCart = await GetCart(Request.Cookies["buyerId"]);

            if (anonCart != null)
            {
                if (userCart != null)
                {
                    _context.Carts.Remove(userCart);
                }

                anonCart.BuyerId = user.UserName;
                Response.Cookies.Delete("buyerId");
                await _context.SaveChangesAsync();
            }
            var userDto = new UserDTO
            {
                Email = user.Email,
                Username = user.UserName,
                Token = await _tokenService.GenerateToken(user),
                Cart = anonCart != null ? anonCart.MapCartDto() : userCart?.MapCartDto()
            };

            return userDto;
        }


        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerModel)
        {
            try
            {
                var user = new User { UserName = registerModel.Username, Email = registerModel.Email, };

                var result = await _userManager.CreateAsync(user, registerModel.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return ValidationProblem();
                }

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                SendEmailConfirmation(user, token, "Confirmation Email", "verifyEmail");
                await _userManager.AddToRoleAsync(user, "Member");

                return Ok(token);

            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Problem Registering User");

                return null;
            }





        }


        [HttpGet("verifyEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                    return BadRequest(new ProblemDetails { Title = "User not found" });

                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return ValidationProblem();
                }


                return Ok(new { message = "Email Verified Successfully" });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Could not verify {email}");
                return StatusCode(500);
            }
        }


        [HttpPost("forgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest();

                var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
                if (user == null)
                    return BadRequest(new ProblemDetails { Title = "User not found" });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                SendEmailConfirmation(user, token, "Reset password token", "ResetPassword");


                return Ok(new { token = token });
            }
            catch(Exception  ex)
            {
                _logger.LogError(ex, "Problem sending forgot password email");
                return null; ;
            }
            
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                return BadRequest(new ProblemDetails { Title = "User not found" });

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);

            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return BadRequest(new ProblemDetails { Title = "Password Reset Failed" });
            }
            return Ok(StatusCode(200));
        }


        [HttpGet("getproviders")]
        public async Task<ActionResult> GetExternalAuthenticationScheme()
        {

            var providers = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!providers.Any())
            {
                return NotFound();
            }

            var provider = providers.Select(x => new
            {
                name = x.Name,
                displayName = x.DisplayName,
                handlerType = x.HandlerType.ToString()
            }).ToList();


            return Ok(provider);

        }


        [HttpGet("ExternalLogin")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
           var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            ///api/Account/ExternalLoginCallback?returnUrl=http%3A%2F%2Flocalhost%3A3000

//var redirectUrl = $"http://localhost:3000";s
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            properties.AllowRefresh = true;
            var challenge = Challenge(properties, provider);

            //challenge.Properties.Items["LoginProvider"]
            return Challenge(properties, provider);
        }

        [HttpGet("ExternalLoginCallback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null)
        {
           try{
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return NotFound(new ProblemDetails { Title = "Could not find user login info" });
            }
            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (signInResult.Succeeded)
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

             

                var infoDTO = await ExternalLoginConfirmationAsync(info);

                infoDTO.Cart = null;


                var cookieOptions = new CookieOptions {
                 IsEssential = true, Expires = DateTime.Now.AddMinutes(5) };


               

                Response.Cookies.Append("userLoginInfo", JsonConvert.SerializeObject(infoDTO, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    },
                    Formatting = Formatting.Indented
                }), cookieOptions);

                /*var infoDTO = new ExternalLoginModel
                {
                    Email = email,
                    UserName =  info.Principal.Identity.Name.Split(" ")[0] ?? email,
                    info = new UserLoginInfoModel{
                        ProviderKey = info.ProviderKey,
                        LoginProvider = info.LoginProvider,
                        ProviderDisplayName = info.ProviderDisplayName
                    }
                };
                */
                //
              //  return Ok(infoDTO);
                return LocalRedirect("/");

            }
            if (signInResult.IsLockedOut)
            {
                return BadRequest(new ProblemDetails { Title = "This account has been locked. If you forgot your password, click on forgot password to reset your Password." });
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
              //  return Ok(new { Provider = info.LoginProvider, returnUrl = //returnUrl, Email = email });
              //return Redirect(returnUrl);
              
                var infoDTO = await ExternalLoginConfirmationAsync(info);
             infoDTO.Cart = null;
                var cookieOptions = new CookieOptions { 
                    IsEssential = true, Expires = DateTime.Now.AddMinutes(5) };
    
                Response.Cookies.Append("userLoginInfo", JsonConvert.SerializeObject(infoDTO, new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    },
                    Formatting = Formatting.Indented
                }), cookieOptions);

               // return Ok(infoDTO);
           //  return Redirect(returnUrl);
          return LocalRedirect("/");
            }
           } catch(Exception error){
            _logger.LogError(error.Message);
            return StatusCode(500);
           }
        }


        [HttpPost("extLoginConfirmation")]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(model);
            var info = new UserLoginInfo(model.info.LoginProvider, model.info.ProviderKey, model.info.ProviderDisplayName);



            var user = await _userManager.FindByEmailAsync(model.Email);
            IdentityResult result;

            if (user != null)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new { response = "SignIn" });
                }
            }
            //var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
                return NotFound(new ProblemDetails { Title = "Could not find user login info" });


            else if (info != null)
            {
                // model.Principal = info.Principal;
                //user = _mapper.Map<User>(model);
                user = new User { Email = model.Email, UserName = model.UserName, EmailConfirmed = true };
                result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        //TODO: Send an emal for the email confirmation and add a default role as in the Register action

                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        SendEmailConfirmation(user, token, "Confirmation Email", "ConfirmEmail");

                        await _userManager.AddToRoleAsync(user, "Member");

                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //user details

                        var userCart = await GetCart(user.UserName);

                        var anonCart = await GetCart(Request.Cookies["buyerId"]);

                        if (anonCart != null)
                        {
                            if (userCart != null)
                            {
                                _context.Carts.Remove(userCart);
                            }

                            anonCart.BuyerId = user.UserName;
                            Response.Cookies.Delete("buyerId");
                            await _context.SaveChangesAsync();
                        }
                        var userDTO = new UserDTO
                        {
                            Email = user.Email,
                            Username = user.UserName,
                            Token = await _tokenService.GenerateToken(user),
                            Cart = anonCart != null ? anonCart.MapCartDto() : userCart?.MapCartDto()
                        };


                        var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddMinutes(5) };

                        Response.Cookies.Append("user", JsonConvert.SerializeObject(userDTO, new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy()
                            },
                            Formatting = Formatting.Indented
                        }), cookieOptions);



                        return Ok(userDTO);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
            }


            return Ok(new { response = "ExternalLogin" });
        }

        private async Task<UserDTO> ExternalLoginConfirmationAsync(ExternalLoginInfo info, string returnUrl = null)
        {
            UserDTO userDTO = null;

            if (info == null)
            {
                return null;
            }

            var signinResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (signinResult.Succeeded)
            {
                /* var jwtResult = await jwtAuthManager.GenerateTokens(user, claims, DateTime.UtcNow);

                 await userManager.SetAuthenticationTokenAsync(
                     user,
                     TokenOptions.DefaultProvider,
                     appSettings.RefreshTokenName,
                     jwtResult.RefreshToken);

                 loginResult = new LoginResultViewModel()
                 {
                     User = new UserViewModel()
                     {
                         Email = email,
                         AccessToken = jwtResult.AccessToken,
                         RefreshToken = jwtResult.RefreshToken,
                         FirstName = user.FirstName,
                         LastName = user.LastName,
                         Phone = user.PhoneNumber,
                         UserId = user.Id
                     }
                 };*/

                var userCart = await GetCart(user.UserName);

                var anonCart = await GetCart(Request.Cookies["buyerId"]);

                if (anonCart != null)
                {
                    if (userCart != null)
                    {
                        _context.Carts.Remove(userCart);
                    }

                    anonCart.BuyerId = user.UserName;
                    Response.Cookies.Delete("buyerId");
                    await _context.SaveChangesAsync();
                }
                userDTO = new UserDTO
                {
                    Email = user.Email,
                    Username = user.UserName,
                    Token = await _tokenService.GenerateToken(user),
                    Cart = anonCart != null ? anonCart.MapCartDto() : userCart?.MapCartDto()
                };

                return userDTO;
            }


            if (!String.IsNullOrEmpty(email))
            {
                // model.Principal = info.Principal;
                //user = _mapper.Map<User>(model);

                var userName = email.Split('@')[0];

                if (user == null)
                {
                    user = new User()
                    {
                        UserName = userName,
                        Email = email,
                        EmailConfirmed = true
                    };
                    await _userManager.CreateAsync(user);
                }

                await _userManager.AddLoginAsync(user, info);
                await _signInManager.SignInAsync(user, false);
                //var jwtResult = await jwtAuthManager.GenerateTokens(user, claims, DateTime.UtcNow);

                //sucess
                var userCart = await GetCart(user.UserName);

                var anonCart = await GetCart(Request.Cookies["buyerId"]);

                if (anonCart != null)
                {
                    if (userCart != null)
                    {
                        _context.Carts.Remove(userCart);
                    }

                    anonCart.BuyerId = user.UserName;
                    Response.Cookies.Delete("buyerId");
                    await _context.SaveChangesAsync();
                }
                userDTO = new UserDTO
                {
                    Email = user.Email,
                    Username = user.UserName,
                    Token = await _tokenService.GenerateToken(user),
                    Cart = anonCart != null ? anonCart.MapCartDto() : userCart?.MapCartDto()
                };

                return userDTO;



            }





            return null;
        }




        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            Console.WriteLine(User.Identity.Name);

            if (user == null) return null;

            var userCart = await GetCart(user.UserName);



            var userDto = new UserDTO
            {
                Email = user.Email,
                Username = user.UserName,
                Token = await _tokenService.GenerateToken(user)
                ,
                Cart = userCart?.MapCartDto()
            };

            return userDto;
        }

        [HttpGet("savedAddress")]

        public async Task<ActionResult<UserAddress>> GetSavedAddress()
        {
            var address = await _userManager.Users.Where(x => x.UserName == User.Identity.Name).Select(user => user.Address).FirstOrDefaultAsync();


            return address;
        }

        private async Task<Cart> GetCart(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId))
            {
                Response.Cookies.Delete("buyerId");
                return null;
            }
            var cartExist = await _context.Carts
            .Include(i => i.Items)
            .ThenInclude(p => p.Product)
            .OrderByDescending(x => x.BuyerId.ToLower() == buyerId.ToLower()).FirstOrDefaultAsync();


            return cartExist;
        }


        private string GenerateRandomOTP(int iOTPLength, string[] saAllowedCharacters)
        {

            string sOTP = String.Empty;

            string sTempChars = String.Empty;

            Random rand = new Random();

            for (int i = 0; i < iOTPLength; i++)

            {

                int p = rand.Next(0, saAllowedCharacters.Length);

                sTempChars = saAllowedCharacters[rand.Next(0, saAllowedCharacters.Length)];

                sOTP += sTempChars;

            }

            return sOTP;
        }

        private async void SendEmailConfirmation(User user, string token, string subject, string action)
        {

            var callback = "";

            var messageContent = new MessageContent
            {
                Subject = subject,
                UserName = user.UserName,
                Token = token,
                EmailType = action
            };
            // var callback = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);
              

            var hasOrigin = this.Request.Headers.TryGetValue("Origin", out var origin);

            if (hasOrigin)
            {
                 callback = string.Format("{0}/{1}/{2}/{3}", origin, action, token, user.Email);
            }
            else
            {
                callback = Url.Action(action, "Account", new { token, email = user.Email }, Request.Scheme);
            }

           

            var message = new Message(new string[] { user.Email }, "haryorbumz@gmail.com", callback, messageContent, null);
           
         
            await _emailService.SendEmailAsync(message);
        }

        private CookieOptions setCookieOptions(UserDTO infoDTO){
             var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddMinutes(5) };

                      return cookieOptions;
        }




    }







}