using CardMarket_Web_Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    #region ProductObj

    /* Product is a card, will get X objects where X is the no. products returned on 
       the main search screen, blade of selves has had two reprints and no extra
       related products, so we get two product objects
         */
    public class ProductObj
    {
        public bool storedOnDb;
        public string cardName;

        public List<Product> product;
        public List<Link> links;

        public ProductObj()
        {
            product = new List<Product>();
        }
    }


    public class Product
    {
        public string enName { get; set; }
        public int idProduct { get; set; }
        public int idMetaproduct { get; set; }
        public string expansionName { get; set; }

        public int countReprints { get; set; }

        public string locName { get; set; }
        public List<Localization> localization { get; set; }
        public string website { get; set; }
        public string image { get; set; }
        public string gameName { get; set; }
        public string categoryName { get; set; }
        public int idGame { get; set; }

        /* some articles have a range as a number i.e. 
         * Cabal Caffers is 2007-10
         * */
        public string number { get; set; }
        public string rarity { get; set; }

        public int expansionIcon { get; set; }
        public int countArticles { get; set; }
        public int countFoils { get; set; }
    }

    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
        public string method { get; set; }
    }

    public class Localization
    {
        public string name { get; set; }
        public string idLanguage { get; set; }
        public string languageName { get; set; }
    }
    #endregion

    #region ArticleObj

    /* Articles are cards a user is selling*/
    public class ArticleObj
    {
        public List<Article> article;
    }

    public class Seller
    {
        public int idUser { get; set; }
        public string username { get; set; }
        public int isCommercial { get; set; }
        public int reputation { get; set; }
        public int sellCount { get; set; }
        public bool onVacation { get; set; }
        public Name name { get; set; }
        public Address address { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string vat { get; set; }
        public string registrationDate { get; set; }
        public bool isSeller { get; set; }
        public string legalInformation { get; set; }
        public int unsentShipments { get; set; }
        public int shipsFast { get; set; }
        public int soldItems { get; set; }
        public int avgShippingTime { get; set; }
        public int riskGroup { get; set; }
    }

    public class Name
    {
        public string firstName { get; set; }
    }

    public class Address
    {
        public string country { get; set; }
    }

    public class Prices
    {
        public float price { get; set; }
        public int idCurrency { get; set; }
        public string currencyCode { get; set; }
    }

    public class Language
    {
        public int idLanguage { get; set; }
        public string languageName { get; set; }
    }
    #endregion
}
