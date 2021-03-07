using CardMarket_Web_Core.ApiQueryLogic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MySql.Data.MySqlClient;

namespace CardMarket_Web_Core.DbCode
{
    public class DAO2
    {

        MySqlConnection conn;
        string myConnectionString = "server=127.0.0.1;uid=root;" +
                                    "pwd=Chicken123;database=cardmarketdb";

        public bool HasProductInfo(string cardName, out ProductObj retProducObj)
        {
            //Console.WriteLine("HasProductInfo: " + cardName);

            string sqlStatement = "select * from Product where cardname = @cardname";
            retProducObj = new ProductObj();

            using (conn = new MySqlConnection(myConnectionString))
            {

                try
                {
                    conn.Open();
                    //Console.WriteLine($"MySQL version : {conn.ServerVersion}");

                    MySqlCommand myCommand = new MySqlCommand(sqlStatement, conn);

                    myCommand.Parameters.AddWithValue("@cardname",cardName);

                    MySqlDataReader myReader;

                    myReader = myCommand.ExecuteReader();

                    while (myReader.Read())
                    {
                        //Console.WriteLine(myReader["cardname"]);

                        Product p = new Product()
                        {
                            enName        = myReader["cardname"].ToString().Trim(),
                            idProduct     = (int)myReader["productid"],
                            idMetaproduct = (int)myReader["metaproductid"],
                            expansionName = myReader["expansionname"].ToString().Trim()
                        };

                        retProducObj.product.Add(p);
                    }

                    if (retProducObj != null)
                    return true;
                    else return false;

                }
                catch (MySql.Data.MySqlClient.MySqlException ex)
                {
                    Console.WriteLine(ex.Message);
                    retProducObj = null;
                    return false;
                }            
            }
        }
    }
}
