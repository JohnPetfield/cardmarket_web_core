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
            return View();
        }

        public IActionResult ApiQuery()
        {

            Input input = new Input
            {
                cardNamesString = "Shadowborn Apostle"
            };

            return View("ApiQuery", input);
        }

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

                return View("ViewOrders", apiQueryModel);
            }
            catch (AggregateException ae)
            {
                /// <summary>
                /// https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-handle-exceptions-in-parallel-loops
                /// </summary>

                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    if (ex is WebException)
                    {
                        Console.WriteLine("caught webexception - cardmarket conroller");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Exception Type: " + ex.GetType());
                        ViewBag.ErrorTitle = "webexception";
                        ViewBag.ErrorMessage = "CardMarket's website has timed out.";
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
