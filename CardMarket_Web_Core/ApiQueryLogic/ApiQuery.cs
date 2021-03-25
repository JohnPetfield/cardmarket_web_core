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

            Console.WriteLine("ApiQuery - started");

            #region runQuery
            
            /// Prevents "Unhandled Exception: System.Net.WebException: 
            /// The request was aborted: Could not create SSL/TLS secure channel." error
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            
            Console.WriteLine("Single Country only? " + inputObj.singleCountryOnly);
            if (inputObj.singleCountryOnly)
            {
                Console.WriteLine("Country Code:- " + inputObj.countryCode);
            }

            ///    Each product returns articles from users, a product is a reprint so I'm going to 
            ///    consolidate all the lists of Articles for each product into a single list of 
            ///    Articles for all products available for a metaproductID

            /// getData 
            /// INPUTS   List<string>        inputObj.cardNamesList
            /// OUTPUTS  List<List<Article>> articlesConsolidatedUsingMetaproductId


            List<List<Article>> articlesConsolidatedUsingMetaproductId = new List<List<Article>>();

            var exceptions = new ConcurrentQueue<Exception>();

            #region getData
            try
            {
                List<Product> products = iDao.GetAllProductsFromDB(inputObj.cardNamesList);

                List<ProductObj> productObjs = MyListOps.ProductObjListFromUnsortedProductObj(products, inputObj.cardNamesList);

                /*
                Console.WriteLine("=========");
                Console.WriteLine("new DAO method - start");
                Console.WriteLine("=========");
                int i = 0;
                foreach (ProductObj productObj in productObjs)
                {
                    Console.WriteLine(i);
                    i++;
                    foreach (Product product in productObj.product)
                    {
                        Console.WriteLine(product.enName + " - " + product.expansionName + " - " + product.idProduct);
                    }
                }

                Console.WriteLine("new DAO method - end");
                Console.WriteLine("=========");
                */

                ApiCall apiCall = new ApiCall();
                articlesConsolidatedUsingMetaproductId = apiCall.Run(inputObj, iDao, productObjs);
                
                if(apiCall.productObjsToSaveToDb != null && 
                   apiCall.productObjsToSaveToDb.Count > 0)
                {
                    Console.WriteLine("about to save all productsDB");
                    iDao.SaveAllProductsDB(apiCall.productObjsToSaveToDb);
                }
                /// ApiCall to have a list of ProductObj,
                /// when I'm currently calling idao.AddProduct, this should simple add the productObj
                /// to the list, then here after apiCall.Run has finished we can get the list and pass it to the 
                /// DAO to save all products in one go.

            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }

            if (exceptions.Count > 0) throw new AggregateException(exceptions);
            #endregion

            /// organiseData 
            /// INPUTS : List<List<Article>> articlesConsolidatedUsingMetaproductId
            /// OUTPUTS: List<Order>         potentialOrders

            /// Organise data takes the data from API calls and does all the 
            /// processing / grouping / filtering to create the orders and order them

            return MyListOps.OrganiseDataAfterApiCalls(articlesConsolidatedUsingMetaproductId,inputObj);
            #endregion
        }
    }
}
