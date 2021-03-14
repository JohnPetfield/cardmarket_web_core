using System;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;

namespace CardMarket_Web_Core.ApiQueryLogic
{

    class RequestHelper
    {
        private readonly Dictionary<string, string> tokens;

        public RequestHelper()
        {
            tokens = Tokens.ReadTokens();
        }

        public string MakeRequest(string url)
        {
            String method = "GET";

            HttpWebRequest request = WebRequest.CreateHttp(url) as HttpWebRequest;

            // Sometimes it timesout this seems a bit of a hack but 
            // was a suggestion I found online
            request.Timeout = 1000000;


            OAuthHeader header = new OAuthHeader(tokens);
            request.Headers.Add(HttpRequestHeader.Authorization, header.getAuthorizationHeader(method, url));
            request.Method = method;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                //Console.WriteLine("response.Headers: " + response.Headers);

                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        String responseString = reader.ReadToEnd();

                        //Console.WriteLine("response.StatusCode: " + response.StatusCode);
                        //Console.WriteLine(responseString);

                        // again don't know if these do anything to
                        // help with the timeout issue

                        response.Close();
                        response.Dispose();

                        return responseString;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }
    }
}
