using CardMarket_Web_Core.ApiQueryLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.Models
{
    public class Order
    {
        [DisplayName("Seller:")]
        public string user { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2} €")]
        [DisplayName("Total Cost:")]
        public float totalCost { get; set; }

        [DisplayName("Articles:")]
        public List<Article> articles { get; set; }

        [DisplayName("Shipping Location:")]
        public string sellerLocation { get; set; }

        public Order()
        {
            articles = new List<Article>();
            sellerLocation = "";
        }
    }

    public class Article
    {
        // metaproduct ID added by me not in 
        // returned Article JSON data
        public int idMetaproduct { get; set; }
        public string enName { get; set; }
        public int idArticle { get; set; }
        public int idProduct { get; set; }

        public string comments { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2} €")]
        [DisplayName("Price:")]
        public float price { get; set; }
        public Seller seller { get; set; }
        public int idCurrency { get; set; }
        public string currencyCode { get; set; }
        public int count { get; set; }
        public bool inShoppingCart { get; set; }
        public string condition { get; set; }
        public bool isFoil { get; set; }
        public bool isSigned { get; set; }
        public bool isPlayset { get; set; }
        public bool isAltered { get; set; }
    }
}
