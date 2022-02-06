using CardMarket_Web_Core.ApiQueryLogic;
using CardMarket_Web_Core.DbCode;
using CardMarket_Web_Core.Exceptions;
using CardMarket_Web_Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static CardMarket_Web_Core.ApiQueryLogic.APIQuery;

namespace CardMarket_Web_Core.Controllers
{
    public class CardMarketController : Controller
    {
        readonly IConfiguration config;
        private readonly IDAO iDAO;

        public CardMarketController(IConfiguration _config)
        {
            config = _config;
            string env = config["ENV"];
            string connectionString =  config.GetConnectionString("DefaultConnection");

            if (env == "Pi")
            {
                iDAO = new DAOMySQL(connectionString); 
            }
            else
            {
                iDAO = new DAOSQL(connectionString); 
            }
        }

        public IActionResult Index()
        {
            //return View();
            Input input = new Input
            {
                cardNamesString = "Shadowborn Apostle"
            };

            return View("findsellers", input);
        }

        public string test()
        {
            return "test message";
        }
        /*

        public IActionResult findsellers()
        {

            Input input = new Input
            {
                cardNamesString = "Shadowborn Apostle"
            };

            return View("findsellers", input);
        }*/

        public ActionResult ViewOrders(Input input)
        {
            try
            {
                input.PrepareInput();

                
                APIQuery query = new APIQuery(input);

                ApiQueryModel apiQueryModel = new ApiQueryModel
                {
                    orders = query.RunQuery(iDAO)
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

                return View("ViewOrders", apiQueryModel) ;
            }
            catch (AggregateException ae)
            {
                /// https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops

                foreach (var ex in ae.Flatten().InnerExceptions)
                {

                    if (ex is WebException)
                    {
                        Console.WriteLine("caught webexception - cardmarket conroller");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Exception Type: " + ex.GetType());
                        ViewBag.ErrorTitle = "CardMarket Error" + ex.GetType();
                        ViewBag.ErrorMessage = "CardMarket's website has returned an error. " + ex.Message;
                        return View("Error");
                    }
                    
                    else if (ex is CardNotFoundException)
                    {
                        Console.WriteLine("caught CardNotFoundException - cardmarket conroller");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Exception Type: " + ex.GetType());
                        ViewBag.ErrorTitle = "Card Name";
                        ViewBag.ErrorMessage = ex.Message;
                        return View("Error");
                    }
                    else
                    {
                        Console.WriteLine("caught general exception - cardmarket conroller");
                        Console.WriteLine("exception message " + ex.Message);
                        return View("Error");
                    }
                }
                return View("Error");
            }
        }
    }
}
