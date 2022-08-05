using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AnimeBack.Data;
using AnimeBack.Entities;
using AnimeBack.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnimeBack
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using var scope = host.Services.CreateScope();
            // var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();

            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            try
            {
                await context.Database.MigrateAsync();
                await DbInitializer.InitializeProduct(context, userManager, config);


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Problem Migrating Database");
            }

            await host.RunAsync();
        }

        private static bool IsDevelopment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";





        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    bool IsDevelopment =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

                    string HostPort =
                        IsDevelopment
                            ? "5000"
                            : Environment.GetEnvironmentVariable("PORT");

                    Console.WriteLine($"Port: {HostPort}");


                 


                    if(!IsDevelopment)
                    {webBuilder
                   .UseUrls($"http://+:{HostPort};")
                    .UseStartup<Startup>();}
                    else{
                       webBuilder.UseStartup<Startup>(); 
                    }
                   // https://+:{HostPort};
                });
    }
}
