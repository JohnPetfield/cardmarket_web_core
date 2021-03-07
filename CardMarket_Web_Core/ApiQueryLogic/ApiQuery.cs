using CardMarket_Web_Core.DbCode;
using CardMarket_Web_Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    public class ApiQuery
    {
        public class APIQuery
        {
            Input inputObj;
            public APIQuery(Input _inputObj)
            {
                inputObj = _inputObj;
            }

            public List<Order> RunQuery()
            {
                #region runQuery
                /*Prevents "Unhandled Exception: System.Net.WebException: 
                  The request was aborted: Could not create SSL/TLS secure channel." error*/
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                RequestHelper myRequest = new RequestHelper();

                Console.WriteLine("======");
                Console.WriteLine("Single Country only? " + inputObj.singleCountryOnly);
                if (inputObj.singleCountryOnly)
                {
                    Console.WriteLine("Country Code:- " + inputObj.countryCode);
                }
                Console.WriteLine("======");

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

                #region getData
                Parallel.ForEach(inputObj.cardNamesList, cardName =>
                {
                    #region wantedCards
                    /** Find all Products for each cardName */

                    //DAO dao = new DAO();
                    
                    
                    DAO2 dao2 = new DAO2();


                    //dao2.HasProductInfo(cardName, out returnProductObj);



                    //ProductObj returnProductObj;

                    ProductObj returnProductObj = new ProductObj();

                    bool haveProductInfo = dao2.HasProductInfo(cardName, out returnProductObj);
                    //bool haveProductInfo = false;

                    String url = "https://api.cardmarket.com/ws/v2.0/output.json/products/find?search="
                               + cardName + "&exact=true&idGame=1&idLanguage=1";

                    if (!haveProductInfo)
                    {
                        Console.WriteLine("Product Info for card: [[{0}]] not held on DB.", cardName);
                        returnProductObj = JsonConvert.DeserializeObject<ProductObj>(myRequest.MakeRequest(url), settings);

                        if (returnProductObj != null &&
                            returnProductObj.product.Count() > 0)
                        {
                            // save to DB
                            //dao.AddProduct(returnProductObj);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Product Info for card: [[{0}]] is held on DB, no API call needed.", cardName);
                    }

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

                            Console.WriteLine("Article API - completed");

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
                        Console.WriteLine("---");
                        Console.WriteLine("Product object null");
                        Console.WriteLine("---");
                        Console.WriteLine("Is card name correct? " + cardName);
                        Console.WriteLine("using exact match so need \"//\" if modal or split card etc");
                    }

                    // Sort each list of Articles in username order
                    cumulativeArticleList.Sort((x, y) => x.seller.username.CompareTo(y.seller.username));

                    articlesConsolidatedUsingMetaproductId.Add(cumulativeArticleList);
                    #endregion
                });  // foreach cardName
                #endregion

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

                foreach (var v in res)
                {
                    Order order = new Order();
                    order.user = v;

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
                        potentialOrders.Add(order);
                    else if (!inputObj.singleCountryOnly)
                        potentialOrders.Add(order);
                }

                potentialOrders = MyListOps.OrderByPrice(potentialOrders);

                if (potentialOrders.Count != 0)
                {
                    // Display the final results i.e. the sorted Orders
                    foreach (Order order in potentialOrders)
                    {
                        Console.WriteLine(order.user + " totalCost: " + order.totalCost);

                        foreach (Article a in order.articles)
                        {
                            Console.WriteLine("    " + a.enName + " "
                                                     + a.idMetaproduct + " "
                                                     + a.price + " "
                                                     + a.currencyCode
                                                     + " Country code: "
                                                     + a.seller.address.country);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("=====");
                    Console.WriteLine("No Sellers found for all cards required");
                    Console.WriteLine("=====");
                }

                return potentialOrders;

                #endregion

            }
        }
    }
}
