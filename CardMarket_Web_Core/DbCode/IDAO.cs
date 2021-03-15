using CardMarket_Web_Core.ApiQueryLogic;

namespace CardMarket_Web_Core.DbCode
{
    public interface IDAO
    {
        void AddProduct(ProductObj productobj);
        bool HasProductInfo(string cardName, out ProductObj retProducObj);
    }
}