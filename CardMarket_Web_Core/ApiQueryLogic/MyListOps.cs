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

        public static List<Order> OrganiseData(List<List<Article>> articlesConsolidatedUsingMetaproductId,
                                                Input inputObj)
        {

            Console.WriteLine("MyListOps.OrganiseData");

            var all = OrderListOfLists(articlesConsolidatedUsingMetaproductId);

            ///compareResults filters out sellers who don't have all the wanted cards for sale,
            ///    leaving a list of sellers who I can purchase all the cards from

            var compareResults = (from list in all
                                    from article in list
                                    where all.All(l => l.Any(o => o.seller.username == article.seller.username))
                                    orderby article.seller.username
                                    select article).ToList();

            // returns a string of all groups (where they are grouped by username)
            // so basically a list of sellers, duplicates removed
            /// https://stackoverflow.com/questions/847066/group-by-multiple-columnsc
            var res = (from a in compareResults
                        group a by a.seller.username
                        into g
                        select g.Key).ToList();

            // An 'order' is a list of each card with the lowest priced article with total cost from a seller
            List<Order> potentialOrders = new List<Order>();

            int resultsLimit = 20;
            int resultCnt = 0;

            foreach (var v in res)
            {
                Order order = new Order();
                order.user = v;

                if (resultCnt >= resultsLimit)
                {
                    break;
                }
                string lastCardName = "";
                bool lMatchingCountry = false;
                foreach (Article a in compareResults)
                {
                    if (a.seller.username == v)
                    {
                        if (order.sellerLocation == "")
                        {
                            order.sellerLocation = a.seller.address.country;
                        }
                        if (lastCardName != a.enName)
                        {
                            lastCardName = a.enName;
                            order.articles.Add(a);
                            order.totalCost += a.price;

                            if (a.seller.address.country == inputObj.countryCode)
                                lMatchingCountry = true;
                        }
                    }
                }
                if (inputObj.singleCountryOnly && lMatchingCountry)
                {
                    potentialOrders.Add(order);
                    resultCnt++;
                }
                else if (!inputObj.singleCountryOnly)
                {
                    potentialOrders.Add(order);
                    resultCnt++;
                }
            }

            potentialOrders = MyListOps.OrderByPrice(potentialOrders);

            if (resultCnt >= resultsLimit)
            {
                Console.WriteLine("No of results limited to: " + resultsLimit);
            }
            Console.WriteLine("No. records returned: " + potentialOrders.Count());
            Console.WriteLine("ApiQuery - completed");
            return potentialOrders;
            //return new List<Order>();
        }
    }
}
