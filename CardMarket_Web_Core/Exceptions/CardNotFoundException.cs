using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.Exceptions
{
    public class CardNotFoundException : Exception
    {

        public CardNotFoundException()
        {
        }

        public CardNotFoundException(string msg): base (msg)
        {
            Console.WriteLine("CardNotFoundException constructor");
        }
    }
}
