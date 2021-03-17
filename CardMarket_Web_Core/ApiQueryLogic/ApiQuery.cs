using CardMarket_Web_Core.DbCode;
using CardMarket_Web_Core.Exceptions;
using CardMarket_Web_Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    public class APIQuery
    {
        private IDAO iDao;
        private Input inputObj;

        public APIQuery(Input _inputObj)
        {
            inputObj = _inputObj;
        }

        public List<Order> RunQuery(IDAO _dao)
        {
            iDao = _dao;
            //string connectionString = _connectionString;

            Console.WriteLine("ApiQuery - started");

            #region runQuery
            /*Prevents "Unhandled Exception: System.Net.WebException: 
                The request was aborted: Could not create SSL/TLS secure channel." error*/
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            RequestHelper myRequest = new RequestHelper();

            Console.WriteLine("Single Country only? " + inputObj.singleCountryOnly);
            if (inputObj.singleCountryOnly)
            {
                Console.WriteLine("Country Code:- " + inputObj.countryCode);
            }

            /// https://stackoverflow.com/questions/31813055/how-to-handle-null-empty-values-in-jsonconvert-deserializeobject
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            /*
                Each product returns articles from users, a product is a reprint so I'm going to 
                consolidate all the lists of Articles for each product into a single list of 
                Articles for all products available for a metaproductID
                */

            List<List<Article>> articlesConsolidatedUsingMetaproductId = new List<List<Article>>();

            var exceptions = new ConcurrentQueue<Exception>();

            #region getData
            try 
            { 
                Parallel.ForEach(inputObj.cardNamesList, cardName =>
            {
                #region wantedCards
                /** Find all Products for each cardName */

                ProductObj returnProductObj = new ProductObj();

                bool haveProductInfo = iDao.HasProductInfo(cardName, out returnProductObj);

                String url = "https://api.cardmarket.com/ws/v2.0/output.json/products/find?search="
                            + cardName + "&exact=true&idGame=1&idLanguage=1";

                if (!haveProductInfo)
                {
                    Console.WriteLine("Product Info for card: [[{0}]] not held on DB.", cardName);
                    Console.WriteLine("Product API call - started");
                    returnProductObj = JsonConvert.DeserializeObject<ProductObj>(myRequest.MakeRequest(url), settings);
                    Console.WriteLine("Product API call - ended");

                    if (returnProductObj != null &&
                        returnProductObj.product.Count() > 0)
                    {
                        // save to DB
                        iDao.AddProduct(returnProductObj);
                    }
                }
                else
                {
                    Console.WriteLine("Product Info for card: [[{0}]] is held on DB, no API call needed.", cardName);
                }

                #region productLoop
                List<Article> cumulativeArticleList = new List<Article>();

                if (returnProductObj != null)
                {
                    Parallel.ForEach(returnProductObj.product, p =>
                    {
                        #region productloop
                        Console.WriteLine(p.enName +
                                        " Product ID: " + p.idProduct +
                                        " Metaproduct ID: " + p.idMetaproduct +
                                        " Expansion: " + p.expansionName);

                        /** Find Articles for each given product v2.0 */

                        url = "https://api.cardmarket.com/ws/v2.0/output.json/articles/" + p.idProduct
                            + "?idLanguage=1&isAltered=false&isFoil=false&isSigned=false";
                        //+ "?idLanguage=1";

                        Console.WriteLine("Article API call - started");

                        ArticleObj returnArticleObj = JsonConvert.DeserializeObject<ArticleObj>(myRequest.MakeRequest(url), settings);

                        Console.WriteLine("Article API call - completed");

                        if (returnArticleObj != null)
                        {
                            int articleCount = 0;

                            foreach (Article a in returnArticleObj.article)
                            {
                                // products should be stored in dictionary, then this could be just looked up whenever needed??
                                a.idMetaproduct = p.idMetaproduct;
                                a.enName = p.enName;
                                articleCount++;
                            } // foreach Article

                            //Console.WriteLine("articleCount: " + articleCount);
                            cumulativeArticleList.AddRange(returnArticleObj.article);

                        } // article !=null
                        else
                        {
                            Console.WriteLine("------");
                            Console.WriteLine("returnArticleObj NULL --- enName: " + p.enName +
                                            " idProduct: " + p.idProduct +
                                            " idMetaproduct: " + p.idMetaproduct +
                                            " expansionName: " + p.expansionName);
                            Console.WriteLine("------");
                        }
                        #endregion
                    });
                }
                else
                {
                    Console.WriteLine("throw custom cardnotfound exception");
                    throw new CardNotFoundException("Card: [[" + cardName + "]] not found.");
                }
                #endregion

                // Sort each list of Articles in username order
                cumulativeArticleList.Sort((x, y) => x.seller.username.CompareTo(y.seller.username));

                articlesConsolidatedUsingMetaproductId.Add(cumulativeArticleList);
                #endregion
            });  // Parallel.ForEach cardName
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }

            if (exceptions.Count > 0) throw new AggregateException(exceptions);
            #endregion

            #region organiseData
            var all = MyListOps.OrderListOfLists(articlesConsolidatedUsingMetaproductId);

            /* compareResults filters out sellers who don't have all the wanted cards for sale,
                leaving a list of sellers who I can purchase all the cards from*/

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
            #endregion
            #endregion

        }
    }
}
