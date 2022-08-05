//using System;
//using AnimeBack.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace AnimeBack.DbContexts
//{
//    public class UserContext : DbContext
//    {
//        public UserContext(DbContextOptions<UserContext> options)
//            :base(options)
//        {
//        }

//        public DbSet<User> Users { get; set; }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            modelBuilder.Entity<User>().HasData(
//                new User()
//                {
//                    Id = 1,
//                    FirstName = "Ayobami",
//                    LastName = "Jimoh",
//                    DateOfBirth = new DateTime(2001, 8, 24),
//                    Email = "test@gmail.com",
//                    PhoneNumber = "08063966430",
//                    Username = "testg"
//                },

//                new User()
//                {
//                    Id = 2,
//                    FirstName = "Nancy",
//                    LastName = "Swashbuckler Rye",
//                    DateOfBirth = new DateTime(1668, 5, 21),
//                    Email = "ayo@gmail.com",
//                    PhoneNumber = "09093081534",
//                    Username = "ayog"
//                },

//                new User()
//                {
//                    Id = 3,
//                    FirstName = "Eli",
//                    LastName = "Ivory Bones Sweet",
//                    DateOfBirth = new DateTime(1701, 12, 16),
//                    Email = "op@test.com",
//                    PhoneNumber = "034809099",
//                    Username = "optest"
//                },
//                new User()
//                {
//                    Id = 4,
//                    FirstName = "Arnold",
//                    LastName = "The Unseen Stafford",
//                    DateOfBirth = new DateTime(1702, 3, 6),
//                    Email = "rp@test.com",
//                    PhoneNumber = "50555667",
//                    Username = "rptest"
//                }

//                );

//            base.OnModelCreating(modelBuilder);
//        }
//    }
//}
