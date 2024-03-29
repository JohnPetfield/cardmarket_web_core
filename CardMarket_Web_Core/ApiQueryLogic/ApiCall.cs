﻿using CardMarket_Web_Core.Exceptions;
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
    public class ApiCall
    {
        JsonSerializerSettings JsonSerializerSettings;
        RequestHelper myRequest ;

        public List<ProductObj> productObjsToSaveToDb;

        public ApiCall()
        {
            /// https://stackoverflow.com/questions/31813055/how-to-handle-null-empty-values-in-jsonconvert-deserializeobject
            JsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            myRequest = new RequestHelper();
            this.productObjsToSaveToDb = new List<ProductObj>();
        }

        private List<Article> GetArticleFromProductForEach(ProductObj returnProductObj)
        {
            List<Article> cumulativeArticleList = new List<Article>();

            Parallel.ForEach(returnProductObj.product, p =>
            {
                #region productloop
                Console.WriteLine(p.enName +
                                " Product ID: " + p.idProduct +
                                " Metaproduct ID: " + p.idMetaproduct +
                                " Expansion: " + p.expansionName);

                /** Find Articles for each given product v2.0 */
                String url = "https://api.cardmarket.com/ws/v2.0/output.json/articles/" + p.idProduct
                    + "?idLanguage=1&isAltered=false&isFoil=false&isSigned=false"; //+ "?idLanguage=1";

                ArticleObj returnArticleObj = JsonConvert.DeserializeObject<ArticleObj>(myRequest.MakeRequest(url), JsonSerializerSettings);

                if (returnArticleObj != null)
                {
                    int articleCount = 0;

                    foreach (Article a in returnArticleObj.article)
                    {
                        a.idMetaproduct = p.idMetaproduct;
                        a.enName = p.enName;
                        articleCount++;
                    }

                    cumulativeArticleList.AddRange(returnArticleObj.article);
                }
                #endregion
            });
            return cumulativeArticleList;
        }

        public List<List<Article>> Run( Input inputObj, IDAO _iDAO, List<ProductObj> productObjs)
        {
            IDAO iDao = _iDAO;

            List<List<Article>> articlesConsolidatedUsingMetaproductId = new List<List<Article>>();

            var exceptions = new ConcurrentQueue<Exception>();

            Parallel.ForEach(inputObj.cardNamesList, cardName =>
            {
                Console.WriteLine("Top of foreach cardname loop: " + cardName);

                #region wantedCards

                // See if cardname is on DB
                ProductObj productObj = new ProductObj();

                if (productObjs != null)
                {
                    foreach (ProductObj po in productObjs)
                    {
                        if (po != null &&
                            !String.IsNullOrWhiteSpace(po.cardName) &&
                            !String.IsNullOrEmpty(po.cardName) &&
                            po.cardName.StartsWith(cardName))
                        {
                            productObj = po;
                            Console.WriteLine("found [[" + cardName + "]] on DB.");
                        }
                    }
                }

                // Not found on DB, make API call to get product info
                if (productObj == null || productObj.cardName == null)
                {
                    String url = "https://api.cardmarket.com/ws/v2.0/output.json/products/find?search="
                                + cardName + "&exact=true&idGame=1&idLanguage=1";

                    productObj = JsonConvert.DeserializeObject<ProductObj>(myRequest.MakeRequest(url), JsonSerializerSettings);

                    if (productObj != null &&
                        productObj.product.Count() > 0)
                    {
                        // save to DB
                        productObjsToSaveToDb.Add(productObj);
                    }
                }
                #region productLoop

                List<Article> cumulativeArticleList;
                if (productObj != null && productObj.cardName != "")
                {
                    cumulativeArticleList = this.GetArticleFromProductForEach(productObj);
                }
                else
                {
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


