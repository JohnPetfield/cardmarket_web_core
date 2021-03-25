using CardMarket_Web_Core.ApiQueryLogic;
using CardMarket_Web_Core.DAO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CardMarket_Web_Core.DbCode
{
    class DAOSQL :DAObase, IDAO
    {
        private string connectionString;
        //MySqlConnection conn;

        public DAOSQL (string _s)
        {
            connectionString = _s;
        }
            /** 
             * connection string in properties of DB puts 'Initial Catalog' as 'Master'
             * my Database is called 'CardMarketDB' to I changed Master to this CardMarketDB
             * https://www.codeproject.com/Questions/1085601/How-to-fix-invalid-object-name
             * **/

        public void SaveAllProductsDB(List<ProductObj> productObjList)
        {
            string sqlStatement = "insert into dbo.Product (cardname, productid, metaproductid,expansionname) " +
                                  "values(@cardname, @productid, @metaproductid,@expansionname)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (ProductObj productObj in productObjList)
                {

                    foreach (Product p in productObj.product)
                    {
                        SqlCommand command = new SqlCommand(sqlStatement, connection);
                        command.Parameters.Add("@cardname", System.Data.SqlDbType.VarChar, 40).Value = p.enName;
                        command.Parameters.Add("@productid", System.Data.SqlDbType.Int, 40).Value = p.idProduct;
                        command.Parameters.Add("@metaproductid", System.Data.SqlDbType.Int, 40).Value = p.idMetaproduct;
                        command.Parameters.Add("@expansionname", System.Data.SqlDbType.VarChar, 40).Value = p.expansionName;

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public List<Product> GetAllProductsFromDB(List<string> cardNamesList)
        {
            List<Product> retProducObj = new List<Product>();

            /// convert list to comma string
            ///https://stackoverflow.com/questions/799446/creating-a-comma-separated-list-from-iliststring-or-ienumerablestring

            string cardNamesSqlSection = ListToStringForSQL(cardNamesList);

            Console.WriteLine(cardNamesSqlSection);
            //string sqlStatement = "select * from dbo.Product where cardname in (" + cardNames + " )";

            string sqlStatement = "select * from Product where  (" + cardNamesSqlSection + " )";

            Console.WriteLine(sqlStatement);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Product p = new Product()
                    {
                        enName = reader["cardname"].ToString().Trim(),
                        idProduct = (int)reader["productid"],
                        idMetaproduct = (int)reader["metaproductid"],
                        expansionName = reader["expansionname"].ToString().Trim()
                    };

                    Console.WriteLine("DB name: " + p.enName);

                    retProducObj.Add(p);
                }
            }
            return retProducObj;
        }

    }
}