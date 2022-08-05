using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using AnimeBack.Helpers;
using AnimeBack.Services;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using Microsoft.OpenApi.Models;
using AnimeBack.Middleware;
using AnimeBack.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

using AnimeBack.Services.EmailService;
using AnimeBack.Services.EmailTokenProvider;
using AnimeBack.RequestHelpers;
using AnimeBack.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace AnimeBack
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // use sql server db in production and sqlite db in development
            /* if (_env.IsProduction())
              {
                  services.AddDbContext<DataContext>();
              }
              else
              {*/
            // services.AddDbContext<DataContext>();
            // }


            services.AddDbContext<DataContext>(opt =>
             {
                 // opt.UseSqlite(_configuration.GetConnectionString("DefaultConnection"));
                 var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


                 Console.WriteLine(env);

                 string connStr;

                 if (env == "Development")
                 {
                     // Use connection string from file.
                     /* connStr = _configuration.GetConnectionString("WebApiDatabase");
                      opt.UseSqlite(connStr);*/
                     //.GetConnectionString("WebApiDatabase");
                     // connStr = _configuration.GetConnectionString("PG_CONNECTION_STRING");

                     connStr = _configuration.GetConnectionString("WebApiDatabase");
                     Console.WriteLine(connStr);
                     Console.WriteLine("Dev");
                     opt.UseSqlite(connStr);
                     // opt.UseNpgsql(connStr);


                 }
                 else
                 {
                     //pgam ==SPass
                     // Use connection string provided at runtime by Heroku.
                     var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");


                     // Console.WriteLine(connUrl);
                     // Parse connection URL to connection string for Npgsql
                     connUrl = connUrl.Replace("postgres://", string.Empty);
                     var pgUserPass = connUrl.Split("@")[0];
                     var pgHostPortDb = connUrl.Split("@")[1];
                     var pgHostPort = pgHostPortDb.Split("/")[0];
                     var pgDb = pgHostPortDb.Split("/")[1];
                     var pgUser = pgUserPass.Split(":")[0];
                     var pgPass = pgUserPass.Split(":")[1];
                     var pgHost = pgHostPort.Split(":")[0];
                     var pgPort = pgHostPort.Split(":")[1];

                     connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};SSL Mode=Require;Trust Server Certificate=true";

                     //Console.WriteLine(connStr);
                     Console.WriteLine("This is Production");
                     opt.UseNpgsql(connStr);


                     /* connStr = _configuration.GetConnectionString("WebApiDatabase");
                     Console.WriteLine(connStr);
                     Console.WriteLine("Prod");
                     opt.UseSqlite(connStr);*/
                 }

                 // Whether the connection string came from the local development configuration file
                 // or from the environment variable from Heroku, use it to set up your DbContext.

             });



            services.AddCors();
            services.AddIdentityCore<User>(opt =>
            {
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
                opt.Tokens.EmailConfirmationTokenProvider = FourDigitTokenProvider.FourDigitEmail;
                opt.Tokens.PasswordResetTokenProvider = FourDigitTokenProvider.FourDigitEmail;
            })
            .AddSignInManager<SignInManager<User>>()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailConfirmationTokenProvider<User>>("emailconfirmation")
            .AddTokenProvider<FourDigitTokenProvider>(FourDigitTokenProvider.FourDigitEmail);


            services.Configure<DataProtectionTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromHours(2));
            services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
                opt.TokenLifespan = TimeSpan.FromDays(3));

            services.AddScoped<IUserClaimsPrincipalFactory<User>, CustomClaimsFactory>();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTSettings:TokenKey"]));

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                x.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie()
             .AddGoogle("google", opt =>
             {
                 var googleAuthSecret = _configuration["GoogleAuth:ClientSecret"];

                 var googleAuthClient = _configuration["GoogleAuth:ClientId"];


                 opt.ClientId = googleAuthClient;
                 opt.ClientSecret = googleAuthSecret;
                 opt.Scope.Add("profile");
                 opt.SignInScheme = IdentityConstants.ExternalScheme;
             })
        .AddJwtBearer(x =>
            {

                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddIdentityCookies();
            //Cookie Policy needed for External Auth



            services.AddAuthorization();
            services.AddScoped<TokenService>();
            services.AddScoped<PaymentService>();
            services.AddScoped<DataClient>();

            var emailConfig = _configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ImageService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Jwt Auth Header",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"

                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id= "Bearer"
                            },
                            Scheme = "oauth2",
                            Name= "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
            services.AddAutoMapper((typeof(MappingProfiles).Assembly));




        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // app.UseDeveloperExceptionPage();
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }
            // migrate any database changes on startup (includes initial db creation)
            //dataContext.Database.Migrate();



            if (!env.IsDevelopment())
            {

                app.UseHsts();


                app.Use((context, next) =>
                    {
                        // context.Request.Host = new HostString("AnimeBack2.herokuapp.com");
                        // context.Request.PathBase = new PathString("/api/"); //if you need this

                        context.Request.Scheme = "https";
                        return next();
                    });

            }


            app.UseHttpsRedirection();








            app.UseStaticFiles();


            app.UseRouting();


            // global cors policy
            app.UseCors(x => x
                // .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowCredentials()
               .WithOrigins(new string[] { "http://localhost:5000","http://AnimeBack2.herokuapp.com", "https://AnimeBack2.herokuapp.com",
               "https://boxstest.netlify.app",
               "https://localhost:3000",  "http://localhost:3000", "https://reverseflash.netlify.app", "https://animeclans-latest.herokuapp.com", "https://a2f3-102-89-43-34.eu.ngrok.io" })
                .AllowAnyHeader());




            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            endpoints.MapFallbackToController("Index", "Fallback");
            }
            );
        }
    }
}






/*configure strongly typed settings objects
           var appSettingsSection = _configuration.GetSection("AppSettings");
           services.Configure<AppSettings>(appSettingsSection);

           // configure jwt authentication
           var appSettings = appSettingsSection.Get<AppSettings>();

           //secret should be more than 16bytes
           var key = Encoding.ASCII.GetBytes(appSettings.Secret);
           /*services.AddAuthentication(x =>
           {
               x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           })
           .AddJwtBearer(x =>
           {
               x.Events = new JwtBearerEvents
               {
                   OnTokenValidated = context =>
                   {
                       var userService = context.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
                       var userId = int.Parse(context.Principal.Identity.Name);
                       var user = userService.GetById(userId);
                       if (user == null)
                       {
                           // return unauthorized if user no longer exists
                           context.Fail("Unauthorized");
                       }
                       return Task.CompletedTask;
                   }
               };
               x.RequireHttpsMetadata = false;
               x.SaveToken = true;
               x.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(key),
                   ValidateIssuer = false,
                   ValidateAudience = false
               };
           });

           // configure DI for application services
           //services.AddScoped<IUserRepository, UserRepository>();*/