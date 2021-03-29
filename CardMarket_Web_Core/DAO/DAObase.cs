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
            int i = 0;
            
            string output = "";

            foreach (string cardName in cardNamesList)
            {
                i++;
                if (cardName != null && cardName != "")
                {
                    output += ((output == "") ? "" : " OR ") +
                                "cardname LIKE " + "@name" + i.ToString() + "";
                }
            }
            return output;
        }
    }
}
