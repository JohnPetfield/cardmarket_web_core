using CardMarket_Web_Core.ApiQueryLogic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using CardMarket_Web_Core.DAO;

namespace CardMarket_Web_Core.DbCode
{
    public class DAOMySQL : DAObase, IDAO
    {
        private string myConnectionString;
        MySqlConnection conn;

        public DAOMySQL(string _s)
        {
            myConnectionString = _s;
        }

        public List<Product> GetAllProductsFromDB(List<string> cardNamesList)
        {
            List<Product> retProducObj = new List<Product>();

            /// convert list to comma string
            ///https://stackoverflow.com/questions/799446/creating-a-comma-separated-list-from-iliststring-or-ienumerablestring

            string cardNamesSqlSection = this.ListToStringForSQL(cardNamesList);
            //string sqlStatement = "select * from Product where cardname in (" + cardNames + " )";
            string sqlStatement = "select * from Product where  (" + cardNamesSqlSection + " )";

            using (conn = new MySqlConnection(myConnectionString))
            {
                conn.Open();

                MySqlCommand myCommand = new MySqlCommand(sqlStatement, conn);

                MySqlDataReader myReader;

                myReader = myCommand.ExecuteReader();

                while (myReader.Read())
                {
                    Product p = new Product()
                    {
                        enName = myReader["cardname"].ToString().Trim(),
                        idProduct = (int)myReader["productid"],
                        idMetaproduct = (int)myReader["metaproductid"],
                        expansionName = myReader["expansionname"].ToString().Trim()
                    };

                    retProducObj.Add(p);
                }
                return retProducObj;
            }
        }

        public void SaveAllProductsDB(List<ProductObj> productObjList)
        {
            string sqlStatement = "insert into Product (cardname, productid, metaproductid,expansionname) " +
                      "values(@cardname, @productid, @metaproductid,@expansionname)";

            using (MySqlConnection connection = new MySqlConnection(myConnectionString))
            {
                connection.Open();
                foreach (ProductObj productObj in productObjList)
                {

                    foreach (Product p in productObj.product)
                    {
                        MySqlCommand command = new MySqlCommand(sqlStatement, connection);

                        command.Parameters.AddWithValue("@cardname", p.enName);
                        command.Parameters.AddWithValue("@productid", p.idProduct);
                        command.Parameters.AddWithValue("@metaproductid", p.idMetaproduct);
                        command.Parameters.AddWithValue("@expansionname", p.expansionName);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
