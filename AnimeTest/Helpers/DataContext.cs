using System;
using AnimeBack.Entities;
using AnimeBack.Entities.OrderAggregate;
using AnimeBack.Entities.PaymentAggregate;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace AnimeBack.Helpers
{
    public class DataContext : IdentityDbContext<User, Role, int>
    {
        protected readonly IConfiguration Configuration;

        public DataContext(DbContextOptions options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        /*  protected override void OnConfiguring(DbContextOptionsBuilder options)
          {

              bool IsDevelopment =
          Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

          var connectionString ="";

          Console.WriteLine(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

          if (IsDevelopment){
              connectionString = Configuration.GetConnectionString("DB_CONNECTION_STRING");

          }
          else{
              connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
          }



              Console.WriteLine("Checking Environment");

              Console.WriteLine(connectionString);

              //Configuration.GetConnectionString("DB_CONNECTION_STRING")
              // connect to sql server database
              options.UseNpgsql(connectionString);
          }*/

        // public DbSet<UserMain> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        // public DbSet<Category> Categories {get; set;}
        public DbSet<Order> Orders { get; set; }
        //public DbSet<CartItem> CartItems {get; set;}
        public DbSet<Cart> Carts { get; set; }
        public DbSet<WishList> WishLists { get; set; }

        public DbSet<PaymentDetail> PaymentDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            

           builder.Entity<Role>()
            .HasData(
                new Role { Id = 1, Name = "Member", NormalizedName = "MEMBER" },
                new Role { Id = 2, Name = "Admin", NormalizedName = "ADMIN" }
            );

            builder
        .Entity<Discount>()
        .Property(e => e.Active)
        .HasConversion<int>();

            builder
            .Entity<User>()
            .Property(e => e.TwoFactorEnabled)
            .HasConversion<int>();

            builder
            .Entity<User>()
            .Property(e => e.EmailConfirmed)
            .HasConversion<int>();

            builder
            .Entity<User>()
            .Property(e => e.PhoneNumberConfirmed)
            .HasConversion<int>();

            builder
            .Entity<User>()
            .Property(e => e.LockoutEnabled)
            .HasConversion<int>();



            builder.Entity<User>()
            .HasOne(a => a.Address)
            .WithOne()
            .HasForeignKey<UserAddress>(a => a.Id)
            .OnDelete(DeleteBehavior.Cascade);




        }
    }
}
