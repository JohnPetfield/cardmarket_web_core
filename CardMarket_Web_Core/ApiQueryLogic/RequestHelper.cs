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

            request.Timeout = 1000000;

            OAuthHeader header = new OAuthHeader(tokens);
            request.Headers.Add(HttpRequestHeader.Authorization, header.getAuthorizationHeader(method, url));
            request.Method = method;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {

                using (Stream stream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    String responseString = reader.ReadToEnd();

                    response.Close();
                    response.Dispose();

                    return responseString;
                }
            }
        }
    }
}
