using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;
using NekoVampire.Extension.Collections;

namespace NekoVampire.Web.OAuth
{
    class OAuthAccess
    {
        #region
        public string ConsumerKey { get { return consumerKey; } }
        protected string consumerKey;
        public string ConsumerSecret { get { return consumerSecret; } }
        protected string consumerSecret;
        public string RequestTokenURL { get { return requestTokenURL; } }
        protected string requestTokenURL;
        public string AccessTokenURL { get { return accessTokenURL; } }
        protected string accessTokenURL;
        public string AuthorizeURL { get { return authorizeURL; } }
        protected string authorizeURL;

        protected const string OAuthSignatureMethod = "HMAC-SHA1";
        protected const string OAuthVersion = "1.0";

        protected const string OAuthParameterPrefix = "oauth_";
        protected const string OAuthConsumerKeyKey = "oauth_consumer_key";
        protected const string OAuthCallbackKey = "oauth_callback";
        protected const string OAuthVersionKey = "oauth_version";
        protected const string OAuthSignatureMethodKey = "oauth_signature_method";
        protected const string OAuthSignatureKey = "oauth_signature";
        protected const string OAuthTimestampKey = "oauth_timestamp";
        protected const string OAuthNonceKey = "oauth_nonce";
        protected const string OAuthTokenKey = "oauth_token";
        protected const string OAuthTokenSecretKey = "oauth_token_secret";
        protected const string OauthVerifierKey = "oauth_verifier";
        protected const string OAuthCallback = "oob";
        protected readonly Encoding Enc = Encoding.UTF8;

        private string requestToken;
        private SecureString requestTokenSecret;
        private string accessToken;
        private SecureString accessTokenSecret;

        public bool HasRequestToken { get { return requestToken != null; } }
        public bool HasAccessToken { get { return accessToken != null; } }

        public string AccessToken
        {
            get { return accessToken; }
            private set { accessToken = value; }
        }

        public SecureString AccessTokenSecret
        {
            get { return accessTokenSecret; }
            private set { accessTokenSecret = value; }
        }

        private Request req;
        private Random rnd;
        #endregion

        #region
        public OAuthAccess(string consumerKey, string consumerSecret, string requestTokenURL, string accessTokenURL, string authorizeURL)
        {
            this.req = new Request();
            this.rnd = new Random();

            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.requestTokenURL = requestTokenURL;
            this.accessTokenURL = accessTokenURL;
            this.authorizeURL = authorizeURL;
        }

        public OAuthAccess(string consumerKey, string consumerSecret, string requestTokenURL, string accessTokenURL, string authorizeURL, string token, SecureString tokenSecret)
            : this(consumerKey, consumerSecret, requestTokenURL, accessTokenURL, authorizeURL)
        {
            this.AccessToken = token;
            this.AccessTokenSecret = tokenSecret;
        }
        #endregion

        public string GetRequestToken()
        {
            var OAuthParameter = new Dictionary<string, string>();
            OAuthParameter.Add(OAuthConsumerKeyKey, consumerKey);
            OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
            OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
            OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
            OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
            OAuthParameter.Add(OAuthCallbackKey, OAuthCallback);
            OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(requestTokenURL, "GET", OAuthParameter));
            Dictionary<string, string> response = null;
            Dictionary<string, string> header = CreateAuthorizationHeader(requestTokenURL, OAuthParameter);
            try
            {
                req.HttpRequest(requestTokenURL, "GET", default(DateTime), null, Enc, header, stream =>
                {
                    using (StreamReader reader = new StreamReader(stream))
                        response = ParseOAuthResponce(reader.ReadToEnd());
                });
            }
            catch (WebException e)
            {
                using (Stream stream = e.Response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return requestTokenURL + "?" + UriUtility.GetPostData(OAuthParameter, Enc);
                }
            }
            requestToken = response[OAuthTokenKey];
            requestTokenSecret = new SecureString();
            foreach (var c in response[OAuthTokenSecretKey].ToCharArray())
                requestTokenSecret.AppendChar(c);
            requestTokenSecret.MakeReadOnly();

            return authorizeURL + "?" + OAuthTokenKey + "=" + UriUtility.EscapeUriString(requestToken);
        }

        public void GetAccessToken(string pin)
        {
            var OAuthParameter = new Dictionary<string, string>();
            OAuthParameter.Add(OAuthConsumerKeyKey, consumerKey);
            OAuthParameter.Add(OAuthTokenKey, requestToken);
            OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
            OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
            OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
            OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
            OAuthParameter.Add(OauthVerifierKey, pin);
            OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(accessTokenURL, "GET", OAuthParameter, requestTokenSecret));
            Dictionary<string, string> response = null;
            Dictionary<string, string> header = CreateAuthorizationHeader(accessTokenURL, OAuthParameter);
            try
            {
                req.HttpRequest(accessTokenURL, "GET", default(DateTime), null, Enc, header, stream =>
                {
                    using (StreamReader reader = new StreamReader(stream))
                        response = ParseOAuthResponce(reader.ReadToEnd());
                });
            }
            catch (WebException e)
            {
                using (Stream stream = e.Response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return;
                }
            }
            accessToken = response[OAuthTokenKey];
            accessTokenSecret = new SecureString();
            foreach (var c in response[OAuthTokenSecretKey].ToCharArray())
                accessTokenSecret.AppendChar(c);
            accessTokenSecret.MakeReadOnly();

            return;
        }

        protected string GenerateSignature(string url, string method, List<QueryParameter> query, SecureString tokenSecret)
        {
            StringBuilder key = new StringBuilder(UriUtility.EscapeUriString(consumerSecret)).Append("&");
            if (tokenSecret != null)
            {
                var bstr = Marshal.SecureStringToBSTR(tokenSecret);
                key.Append(UriUtility.EscapeUriString(Marshal.PtrToStringBSTR(bstr)));
                Marshal.ZeroFreeBSTR(bstr);
            }
            var key_byte = Encoding.ASCII.GetBytes(key.ToString());
            HMACSHA1 hmac = new HMACSHA1(key_byte);
            query.Sort();
            var signaturebase = String.Format(
                "{0}&{1}&{2}",
                UriUtility.EscapeUriString(method),
                UriUtility.EscapeUriString(url),
                UriUtility.EscapeUriString(UriUtility.GetPostData(query.ToDictionary(value => value.Name,value => value.Value), Enc))
                );
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(signaturebase)));
        }

        #region GenerateSignature オーバーロード
        protected string GenerateSignature(string url, string method, IDictionary<string, string> OAuthParameter)
        {
            return GenerateSignature(url, method, OAuthParameter, null);
        }

        protected string GenerateSignature(string url, string method, IDictionary<string, string> OAuthParameter, SecureString tokenSecret)
        {
            List<QueryParameter> query = new List<QueryParameter>();
            if (OAuthParameter != null)
                foreach (var data in OAuthParameter)
                    query.Add(new QueryParameter(data.Key, data.Value));
            return GenerateSignature(url, method, query, tokenSecret);
        }

        protected string GenerateSignature(string url, string method, IDictionary<string, string> OAuthParameter, IDictionary<string, string> postData, SecureString tokenSecret)
        {
            List<QueryParameter> query = new List<QueryParameter>();
            if (OAuthParameter != null)
                foreach (var data in OAuthParameter)
                    query.Add(new QueryParameter(data.Key, data.Value));
            if (postData != null)
                foreach (var data in postData)
                    query.Add(new QueryParameter(data.Key, data.Value));
            return GenerateSignature(url, method, query, tokenSecret);
        }
        #endregion

        protected Dictionary<string, string> CreateAuthorizationHeader(string url, Dictionary<string, string> OAuthParameter)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            StringBuilder str = new StringBuilder("OAuth ");
            foreach (var data in new SortedDictionary<string, string>(OAuthParameter))
                str.AppendFormat("{0}=\"{1}\",", UriUtility.EscapeUriString(data.Key), UriUtility.EscapeUriString(data.Value));
            str.Remove(str.Length - 1, 1);
            dic.Add("Authorization", str.ToString());
            return dic;
        }

        protected string GenereteNonce()
        {
            return GenereteNonce(24);
        }

        protected string GenereteNonce(uint size)
        {
            StringBuilder str = new StringBuilder();
            for (var i = 0; i < size; i++)
            {
                switch (rnd.Next(3))
                {
                    case 1:
                        str.Append((char)rnd.Next('a', 'z'));
                        break;
                    case 2:
                        str.Append((char)rnd.Next('A', 'Z'));
                        break;
                    default:
                        str.Append((char)rnd.Next('0', '9'));
                        break;
                }
            }
            return str.ToString();
        }

        protected string GenereteTimestamp()
        {
            return GenereteTimestamp(DateTime.Now);
        }

        protected string GenereteTimestamp(DateTime date)
        {
            return ((long)(date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds).ToString();
        }

        protected Dictionary<string, string> ParseOAuthResponce(string response)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var line in response.Split('&'))
            {
                var splitString = line.Split('=');
                if (splitString.Length == 2)
                    dic.Add(Uri.UnescapeDataString(splitString[0]), Uri.UnescapeDataString(splitString[1]));
            }
            return dic;
        }

        public void HttpRequest(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, IDictionary<string, string> headers, Action<Stream> invoker)
        {
            Dictionary<string, string> head = new Dictionary<string,string>(headers);
            Dictionary<string, string> OAuthParameter = new Dictionary<string, string>();
            OAuthParameter.Add(OAuthConsumerKeyKey, ConsumerKey);
            OAuthParameter.Add(OAuthTokenKey, AccessToken);
            OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
            OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
            OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
            OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
            OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(url, "POST", OAuthParameter, postData, accessTokenSecret));
            foreach(var header in CreateAuthorizationHeader(url, OAuthParameter))
                head.Add(header.Key,header.Value);

            try
            {
                req.HttpRequest(url, "POST", since, postData, Enc, head, invoker);
            }
            catch (WebException)
            {
                throw;
            }

        }

        public void HttpRequest(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, IDictionary<string, string> headers, Action<Stream> invoker)
        {
            Dictionary<string, string> head = null;

            if (method == "POST")
            {
                Dictionary<string, string> OAuthParameter = new Dictionary<string, string>();
                OAuthParameter.Add(OAuthConsumerKeyKey, ConsumerKey);
                OAuthParameter.Add(OAuthTokenKey, AccessToken);
                OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
                OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
                OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
                OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
                OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(url, "POST", OAuthParameter, postData, accessTokenSecret));

                if (headers != null)
                {
                    head = new Dictionary<string, string>(headers);
                    head.AddRange
                }
                else
                {
                    head = CreateAuthorizationHeader(url, OAuthParameter);
                }
            }

            try
            {
                req.HttpRequest(url, method, since, postData, Enc, head, invoker);
            }
            catch (WebException)
            {
                throw;
            }
        }



        protected class QueryParameter : IComparable
        {
            public QueryParameter(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }

            public string Name
            {
                get;
                private set;
            }

            public string Value
            {
                get;
                private set;
            }

            #region IComparable メンバー

            public int CompareTo(object obj)
            {
                return this.CompareTo((QueryParameter)obj);
            }

            public int CompareTo(QueryParameter qp)
            {
                return this.Name != qp.Name ? this.Name.CompareTo(qp.Name) : this.Value.CompareTo(qp.Value);
            }

            #endregion
        }
    }
}
