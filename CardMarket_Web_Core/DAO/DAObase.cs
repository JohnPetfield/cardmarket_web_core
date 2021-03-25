using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.DAO
{
    public class DAObase
    {
        public string ListToStringForSQL(List<string> cardNamesList)
        {
            string output = "";

            foreach (string cardName in cardNamesList)
            {
                /// add a comma in front cardname if not the first one
                //output += ((output == "") ? "" : ",")
                  //      + "'" + cardName + "'";

                if (cardName != null && cardName != "")
                {
                    output += ((output == "") ? "" : " OR ") +
                                "cardname LIKE '" + cardName + "%'";
                }
                /// for new like % 
                /// cardname like goes in front of cardname
                /// %' goes after
                /// then an or can go where the comma goes above
            }
            return output;
        }

    }
}
