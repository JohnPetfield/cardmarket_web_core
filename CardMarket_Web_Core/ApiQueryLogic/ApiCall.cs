using CardMarket_Web_Core.Exceptions;
using CardMarket_Web_Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardMarket_Web_Core.DbCode;
using Newtonsoft.Json;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    static public class ApiCall
    {
        static public List<List<Article>> Run( Input inputObj, IDAO _iDAO)
        {
            IDAO iDao = _iDAO;

            List<List<Article>> articlesConsolidatedUsingMetaproductId = new List<List<Article>>();

            RequestHelper myRequest = new RequestHelper();

            var exceptions = new ConcurrentQueue<Exception>();
            /// https://stackoverflow.com/questions/31813055/how-to-handle-null-empty-values-in-jsonconvert-deserializeobject
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };


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

            return articlesConsolidatedUsingMetaproductId;
        }
    }
}
