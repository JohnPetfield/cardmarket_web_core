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

            List<List<Article>> articlesConsolidatedUsingMetaproductId = new List<List<Article>>();

            var exceptions = new ConcurrentQueue<Exception>();

            #region getData
            try
            {
                List<Product> products = iDao.GetAllProductsFromDB(inputObj.cardNamesList);

                List<ProductObj> productObjs = MyListOps.ProductObjListFromUnsortedProductObj(products, inputObj.cardNamesList);
                
                ApiCall apiCall = new ApiCall();
                articlesConsolidatedUsingMetaproductId = apiCall.Run(inputObj, iDao, productObjs);
                
                if(apiCall.productObjsToSaveToDb != null && 
                   apiCall.productObjsToSaveToDb.Count > 0)
                {
                    Console.WriteLine("about to save all productsDB");
                    iDao.SaveAllProductsDB(apiCall.productObjsToSaveToDb);
                }
            }
            catch (Exception e)
            {
                exceptions.Enqueue(e);
            }

            if (exceptions.Count > 0) throw new AggregateException(exceptions);
            #endregion

            return MyListOps.OrganiseDataAfterApiCalls(articlesConsolidatedUsingMetaproductId,inputObj);
            #endregion
        }
    }
}
