using CardMarket_Web_Core.ApiQueryLogic;
using System;
using System.Data.SqlClient;

namespace CardMarket_Web_Core.DbCode
{
    class DAOSQL : IDAO
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

            /*
            readonly string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=CardMarketDB;Integrated Security=True;Connect Timeout=30;" +
                                        "Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            */
        public void AddProduct(ProductObj productobj)
        {
            string sqlStatement = "insert into dbo.Product (cardname, productid, metaproductid,expansionname) " +
                "values(@cardname, @productid, @metaproductid,@expansionname)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                foreach (Product p in productobj.product)
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

        public bool HasProductInfo(string cardName, out ProductObj retProducObj)
        {
            string sqlStatement = "select * from dbo.Product where cardname = @cardname";
            retProducObj = new ProductObj();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(sqlStatement, connection);

                command.Parameters.Add("@cardname", System.Data.SqlDbType.VarChar, 40).Value = cardName;

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    if (!reader.HasRows)
                        return false;

                    while (reader.Read())
                    {
                        Product p = new Product()
                        {
                            enName = reader["cardname"].ToString().Trim(),
                            idProduct = (int)reader["productid"],
                            idMetaproduct = (int)reader["metaproductid"],
                            expansionName = reader["expansionname"].ToString().Trim()
                        };

                        retProducObj.product.Add(p);
                    }

                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else return false;

            }
        }
    }
}