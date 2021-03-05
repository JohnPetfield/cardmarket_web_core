using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.Models
{
    public class ApiQueryModel
    {
        // Used by view to ensure Model has data
        /// https://stackoverflow.com/questions/8448778/mvc3-razor-how-to-check-if-model-is-empty
        public bool Empty
        {
            get { return orders.Count == 0; }
        }

        public List<Order> orders = new List<Order>();

        public ApiQueryModel()
        {

        }
    }
}
