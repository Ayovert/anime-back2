using AnimeBack.DTOs.Product;
using AnimeBack.Entities;
using AnimeBack.Services.EmailService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimeBack.Helpers
{
    public class UtilMethods
    {
      

        public UtilMethods()
        {
           
        }
        public string GetCategory(int categoryId)
        {
            //var category = _context.Categories.ToList();
            string category1 = "Misc";

            var category = Enum.GetName(typeof(CategoryModel), categoryId);





            if (category == null) return null;


            return category1;

        }


        public string GetStatusString(int status)
        {
            var statusStr = "Payment Initiated, Order Not Processsed";
            if (status == 1)
            {
                statusStr = "Payment Failed , Order Not Processed";
            }
            else if (status == 2)
            {
                statusStr = "Payment Successful, Order Proccesed";
            }

            return statusStr;
        }


         
    }
}
