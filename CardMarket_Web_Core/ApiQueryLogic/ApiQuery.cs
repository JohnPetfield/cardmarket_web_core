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
                articlesConsolidatedUsingMetaproductId = ApiCall.Run(inputObj, iDao);
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

            return MyListOps.OrganiseData(articlesConsolidatedUsingMetaproductId,inputObj);
            #endregion
        }
    }
}
