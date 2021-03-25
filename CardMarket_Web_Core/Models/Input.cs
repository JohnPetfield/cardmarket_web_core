using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core.Models
{
    public class Input
    {
        //public List<string> listCardNames;

        /* This is whats input in the form, which is then assigned
         * to the variable cardNamesList*/

        [Required]
        [DisplayName("Card Names:")]
        public string cardNamesString { get; set; }

        [DisplayName("Country Code:")]
        public string countryCode { get; set; }
        public bool singleCountryOnly;

        public List<string> cardNamesList;

        public Input(List<string> _listCardNames, string _countryCode = "")
        {
            countryCode = _countryCode;
            singleCountryOnly = (countryCode != "");
        }

        public Input(string _listCardNames, string _countryCode = "")
        {
            countryCode = _countryCode;
            singleCountryOnly = (countryCode != "");
        }

        public void PrepareInput()
        {
            cardNamesString = cardNamesString.Replace("\r\n", ",");
            cardNamesString = cardNamesString.Trim(',');

            ///https://stackoverflow.com/questions/2235683/easiest-way-to-parse-a-comma-delimited-string-to-some-kind-of-object-i-can-loop
            cardNamesList = cardNamesString.Split(',').Select(sValue => sValue.Trim()).ToList();

            /// https://stackoverflow.com/questions/3069748/how-to-remove-all-the-null-elements-inside-a-generic-list-in-one-go
            cardNamesList.RemoveAll(item => item == "");

            if (countryCode == null)
            {
                singleCountryOnly = false;
            }
            else
            {
                countryCode = countryCode.ToUpper();
                singleCountryOnly = (countryCode != "");
            }
        }

        public Input()
        {
        }
    }
}
