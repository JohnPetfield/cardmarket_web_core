using CardMarket_Web_Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CardMarket_Web_Core.ApiQueryLogic.ApiQuery;

namespace CardMarket_Web_Core.Controllers
{
    public class CardMarketController : Controller
    {
        public IActionResult Index()
        {
            // returns CardMarketView.cshtml
            return View();
        }

        public IActionResult ApiQuery()
        {

            Input input = new Input
            {
                //input.countryCode = "GB";
                cardNamesString = "Shadowborn Apostle"
                //cardNamesString = "Shadowborn Apostle\r\nArcane Signet"
            };

            return View("ApiQuery", input);

            //return View();
        }

        public ActionResult ViewOrders(Input input)
        {
            input.PrepareInput();

            APIQuery query = new APIQuery(input);

            ApiQueryModel apiQueryModel = new ApiQueryModel
            {
                orders = query.RunQuery()
            };



            /*
            List<Article> a = new List<Article>() 
            { 
                new Article{ enName = "Damnation", price = 1.1f, currencyCode = "GB"},
                new Article{ enName = "Kadena",    price = 2.2f, currencyCode = "GB"},
                new Article{ enName = "Volrath",   price = 3.3f, currencyCode = "GB"},
            };

            List<Order> o = new List<Order>
            {
                new Order{user = "seller 1", totalCost = 111f,articles = a},
                new Order{user = "seller 2", totalCost = 222f,articles = a},
                new Order{user = "seller 3", totalCost = 333f,articles = a}
            };

            ApiQueryModel apiModelObj = new ApiQueryModel
            {
                orders = o
            };
            */

            return View("ViewOrders", apiQueryModel);
            //return View();
        }
    }
}
