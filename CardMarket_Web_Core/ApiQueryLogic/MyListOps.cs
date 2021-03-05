using CardMarket_Web_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    public static class MyListOps
    {
        public static List<List<Article>> OrderListOfLists(List<List<Article>> collection)
        {
            //Console.WriteLine("===OrderList===");
            //Console.WriteLine("collection.Count: " + collection.Count);

            List<List<Article>> returnCollection = new List<List<Article>>();

            foreach (List<Article> list in collection)
            {
                List<Article> article = new List<Article>();

                ///https://stackoverflow.com/questions/2779375/order-a-list-c-by-many-fields
                article = list.OrderBy(x => x.seller.username)
                              .ThenBy(x => x.enName)
                              .ThenBy(x => x.price).ToList();

                returnCollection.Add(article);
            }

            // Order the collection of lists
            //  returnCollection.Sort((a, b) => a.Count - b.Count);

            return returnCollection;
        }

        public static List<Order> OrderByPrice(List<Order> list)
        {
            List<Order> returnList = new List<Order>();

            ///https://stackoverflow.com/questions/2779375/order-a-list-c-by-many-fields
            returnList = list.OrderBy(x => x.totalCost)
                          .ToList();

            return returnList;
        }


    }
}
