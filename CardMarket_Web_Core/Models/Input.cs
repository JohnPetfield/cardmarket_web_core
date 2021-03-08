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

        /*
        public Input(List<string> _listCardNames, string _cardNames, string _countryCode = "")
        {
            listCardNames = _cardNames;
            countryCode = _countryCode;
            singleCountryOnly = (countryCode != "");
        }
        */

        public Input(List<string> _listCardNames, string _countryCode = "")
        {
            //cardNamesList = _listCardNames;
            countryCode = _countryCode;
            singleCountryOnly = (countryCode != "");
        }

        public Input(string _listCardNames, string _countryCode = "")
        {
            //cardNamesList = _listCardNames.Split('|').ToList();
            countryCode = _countryCode;
            singleCountryOnly = (countryCode != "");

            /*
            Console.WriteLine("constructor that converts comma list to c# list");

            foreach(string name in listCardNames)
            {
                Console.WriteLine(name);
            }
            */
        }

        public void PrepareInput()
        {
            if (countryCode == null)
            {
                singleCountryOnly = false;
            }
            else
            {
                countryCode = countryCode.ToUpper();
                singleCountryOnly = (countryCode != "");
            }
                //cardNamesList = cardNamesString.Split('|').ToList();

            //https://stackoverflow.com/questions/2245442/split-a-string-by-another-string-in-c-sharp
            // splits a string by word, that 'word' been the new line in the text area 
            cardNamesList = cardNamesString.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
        }

        public Input()
        {
        }

        //public string testStringForTextArea{ get; set; }
    }
}
