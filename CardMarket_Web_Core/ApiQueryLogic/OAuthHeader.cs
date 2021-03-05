using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CardMarket_Web_Core.ApiQueryLogic
{
    class OAuthHeader
    {
        protected string appToken, appSecret, accessToken, accessSecret;
        protected String signatureMethod = "HMAC-SHA1";
        protected String version = "1.0";
        protected IDictionary<String, String> headerParams;

        private readonly Dictionary<string, string> tokens;

        public OAuthHeader(Dictionary<string, string> _tokens)
        {
            tokens = _tokens;

            appToken = tokens["appToken"];
            appSecret = tokens["appSecret"];
            accessToken = tokens["accessToken"];
            accessSecret = tokens["accessSecret"];

            //Console.WriteLine("accessSecret: " + accessSecret);

            String nonce = Guid.NewGuid().ToString("n");
            //String nonce = "53eb1f44909d6";
            String timestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();
            //String timestamp = "1407917892";
            /// Initialize all class members
            this.headerParams = new Dictionary<String, String>();
            this.headerParams.Add("oauth_consumer_key", this.appToken);
            this.headerParams.Add("oauth_token", this.accessToken);
            this.headerParams.Add("oauth_nonce", nonce);
            this.headerParams.Add("oauth_timestamp", timestamp);
            this.headerParams.Add("oauth_signature_method", this.signatureMethod);
            this.headerParams.Add("oauth_version", this.version);
        }

        /// <summary>
        /// Pass request method and URI parameters to get the Authorization header value
        /// </summary>
        /// <param name="method">Request Method</param>
        /// <param name="url">Request URI</param>
        /// <returns>Authorization header value</returns>
        public String getAuthorizationHeader(String method, String url)
        {
            var uriParts = url.Split('?');
            var baseUri = uriParts[0];

            /// Add the realm parameter to the header params
            this.headerParams.Add("realm", baseUri);

            /// Start composing the base string from the method and request URI
            String baseString = method.ToUpper()
                              + "&"
                              + Uri.EscapeDataString(baseUri)
                              + "&";

            var requestParameters = uriParts.Count() > 1 ? uriParts[1].Split('&') : new string[] { };
            foreach (var parameter in requestParameters)
            {
                var parts = parameter.Split('=');
                var key = parts[0];
                var value = parts.Count() > 1 ? parts[1] : "";

                //Console.WriteLine("key:- " + key + "value:- " + value);
                this.headerParams.Add(key, value);
            }

            /// Gather, encode, and sort the base string parameters
            SortedDictionary<String, String> encodedParams = new SortedDictionary<String, String>();
            foreach (KeyValuePair<String, String> parameter in this.headerParams)
            {
                if (false == parameter.Key.Equals("realm"))
                {
                    encodedParams.Add(Uri.EscapeDataString(parameter.Key), Uri.EscapeDataString(parameter.Value));
                }
            }

            /// Expand the base string by the encoded parameter=value pairs
            List<String> paramStrings = new List<String>();
            foreach (KeyValuePair<String, String> parameter in encodedParams)
            {
                paramStrings.Add(parameter.Key + "=" + parameter.Value);
            }
            String paramString = Uri.EscapeDataString(String.Join<String>("&", paramStrings));
            baseString += paramString;

            /// Create the OAuth signature
            String signatureKey = Uri.EscapeDataString(this.appSecret) + "&" + Uri.EscapeDataString(this.accessSecret);

            /*
             * BEFORE
             * 
            HMAC hasher = HMACSHA1.Create();
            hasher.Key = Encoding.UTF8.GetBytes(signatureKey);
            Byte[] rawSignature = hasher.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            */

            //HMAC hasher = HMACSHA1.Create();

            HMAC hasher = new HMACSHA1();


            hasher.Key = Encoding.UTF8.GetBytes(signatureKey);
            Byte[] rawSignature = hasher.ComputeHash(Encoding.UTF8.GetBytes(baseString));

            String oAuthSignature = Convert.ToBase64String(rawSignature);

            /// Include the OAuth signature parameter in the header parameters array
            this.headerParams.Add("oauth_signature", oAuthSignature);

            /// Construct the header string
            List<String> headerParamStrings = new List<String>();
            foreach (KeyValuePair<String, String> parameter in this.headerParams)
            {
                headerParamStrings.Add(parameter.Key + "=\"" + parameter.Value + "\"");
            }
            String authHeader = "OAuth " + String.Join<String>(", ", headerParamStrings);

            //Console.WriteLine("authHeader : " + authHeader);

            return authHeader;
        }
    }
}
