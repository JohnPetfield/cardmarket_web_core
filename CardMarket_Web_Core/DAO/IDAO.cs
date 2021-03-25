using CardMarket_Web_Core.ApiQueryLogic;
using System.Collections.Generic;

namespace CardMarket_Web_Core.DbCode
{
    public interface IDAO
    {
        void SaveAllProductsDB(List<ProductObj> productObjList);
        List<Product> GetAllProductsFromDB(List<string> cardNamesList);
    }
}