using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using NekoVampire.Extension.Collections;

namespace NekoVampire.Web.OAuth
{
    public class OAuthAccess
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

        /// <summary>
        /// RequestToken を取得する。
        /// </summary>
        /// <returns>認証 URL。</returns>
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
                    return requestTokenURL + "?" + RequestUtility.GetPostData(OAuthParameter, Enc);
                }
            }
            requestToken = response[OAuthTokenKey];
            requestTokenSecret = new SecureString();
            foreach (var c in response[OAuthTokenSecretKey].ToCharArray())
                requestTokenSecret.AppendChar(c);
            requestTokenSecret.MakeReadOnly();

            return authorizeURL + "?" + OAuthTokenKey + "=" + RequestUtility.EscapeUriString(requestToken);
        }

        /// <summary>
        /// AccessToken を取得する。
        /// </summary>
        /// <param name="pin">PIN コード</param>
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
            StringBuilder key = new StringBuilder(RequestUtility.EscapeUriString(consumerSecret)).Append("&");
            if (tokenSecret != null)
            {
                var bstr = Marshal.SecureStringToBSTR(tokenSecret);
                key.Append(RequestUtility.EscapeUriString(Marshal.PtrToStringBSTR(bstr)));
                Marshal.ZeroFreeBSTR(bstr);
            }
            var key_byte = Encoding.ASCII.GetBytes(key.ToString());
            HMACSHA1 hmac = new HMACSHA1(key_byte);
            query.Sort();
            var signaturebase = String.Format(
                "{0}&{1}&{2}",
                RequestUtility.EscapeUriString(method),
                RequestUtility.EscapeUriString(url),
                RequestUtility.EscapeUriString(RequestUtility.GetPostData(query.ToDictionary(value => value.Key, value => value.Value), Enc))
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

        protected string GenerateSignature(string url, string method, IDictionary<string, string> OAuthParameter, IDictionary<string, string> queryDictionary, SecureString tokenSecret)
        {
            List<QueryParameter> query = new List<QueryParameter>();
            if (OAuthParameter != null)
                foreach (var data in OAuthParameter)
                    query.Add(new QueryParameter(data.Key, data.Value));
            if (queryDictionary != null)
                foreach (var data in queryDictionary)
                    query.Add(new QueryParameter(data.Key, data.Value));
            return GenerateSignature(url, method, query, tokenSecret);
        }
        #endregion

        protected Dictionary<string, string> CreateAuthorizationHeader(string url, Dictionary<string, string> OAuthParameter)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            StringBuilder str = new StringBuilder("OAuth ");
            foreach (var data in new SortedDictionary<string, string>(OAuthParameter))
                str.AppendFormat("{0}=\"{1}\",", RequestUtility.EscapeUriString(data.Key), RequestUtility.EscapeUriString(data.Value));
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
            if (!this.HasAccessToken || !this.HasRequestToken)
                throw new InvalidOperationException("トークンを取得していません。");

            Dictionary<string, string> head = null;

            var uri = new Uri(url);
            Dictionary<string, string> queryDictionary;
            if (postData != null)
                queryDictionary = new Dictionary<string, string>(postData);
            else
                queryDictionary = new Dictionary<string, string>();

            var queryString = uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);
            if (queryString.Length != 0)
            {
                foreach (string query in queryString.Split('&'))
                {
                    if (query != "")
                        if (query.IndexOf('=') > -1)
                        {
                            string[] qs = query.Split('=');
                            queryDictionary.Add(Uri.UnescapeDataString(qs[0]), Uri.UnescapeDataString(qs[1]));
                        }
                        else
                            queryDictionary.Add(Uri.UnescapeDataString(query), "");
                }
            }

            Dictionary<string, string> OAuthParameter = new Dictionary<string, string>();
            OAuthParameter.Add(OAuthConsumerKeyKey, ConsumerKey);
            OAuthParameter.Add(OAuthTokenKey, AccessToken);
            OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
            OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
            OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
            OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
            OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(url, method, OAuthParameter, queryDictionary, accessTokenSecret));

            head = CreateAuthorizationHeader(url, OAuthParameter);
            if (headers != null)
                head.AddRange(headers);

            try
            {
                req.HttpRequest(url, method, since, postData, Enc, head, invoker);
            }
            catch (WebException)
            {
                throw;
            }
        }

        #region HttpRequest オーバロード
        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        public void HttpRequest(string url, string method)
        {
            HttpRequest(url, method, default(DateTime), null, null);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        public void HttpRequest(string url, string method, DateTime since)
        {
            HttpRequest(url, method, since, null, null);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, Action<Stream> invoker)
        {
            HttpRequest(url, method, default(DateTime), null, invoker);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, DateTime since, Action<Stream> invoker)
        {
            HttpRequest(url, method, since, null, invoker);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="postData">POSTで送信するデータ</param>
        public void HttpRequest(string url, string method, IDictionary<string, string> postData)
        {
            HttpRequest(url, method, default(DateTime), postData, null);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, IDictionary<string, string> postData, Action<Stream> invoker)
        {
            HttpRequest(url, method, default(DateTime), postData, invoker);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, DateTime since, IDictionary<string, string> postData, Action<Stream> invoker)
        {
            HttpRequest(url, method, since, postData, Encoding.Default, invoker);
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="enc">文字エンコーディング</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, Action<Stream> invoker)
        {
            HttpRequest(url, method, since, postData, enc, null, invoker);
        }
        #endregion

        public void HttpRequestAsync(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, IDictionary<string, string> headers, Action<Stream> invoker)
        {
            Dictionary<string, string> head = null;

            var uri = new Uri(url);
            Dictionary<string, string> queryDictionary;
            if (postData != null)
                queryDictionary = new Dictionary<string, string>(postData);
            else
                queryDictionary = new Dictionary<string, string>();
            foreach (string query in uri.GetComponents(UriComponents.Query, UriFormat.UriEscaped).Split('&'))
            {
                if (query.IndexOf('=') > -1)
                {
                    string[] qs = query.Split('=');
                    queryDictionary.Add(Uri.UnescapeDataString(qs[0]), Uri.UnescapeDataString(qs[1]));
                }
                else
                    queryDictionary.Add(Uri.UnescapeDataString(query), "");
            }

            Dictionary<string, string> OAuthParameter = new Dictionary<string, string>();
            OAuthParameter.Add(OAuthConsumerKeyKey, ConsumerKey);
            OAuthParameter.Add(OAuthTokenKey, AccessToken);
            OAuthParameter.Add(OAuthSignatureMethodKey, OAuthSignatureMethod);
            OAuthParameter.Add(OAuthTimestampKey, GenereteTimestamp());
            OAuthParameter.Add(OAuthNonceKey, GenereteNonce());
            OAuthParameter.Add(OAuthVersionKey, OAuthVersion);
            OAuthParameter.Add(OAuthSignatureKey, GenerateSignature(url, method, OAuthParameter, queryDictionary, accessTokenSecret));

            head = CreateAuthorizationHeader(url, OAuthParameter);
            head.AddRange(headers);

            try
            {
                req.HttpRequestAsync(url, method, since, postData, Enc, head, invoker);
            }
            catch (WebException)
            {
                throw;
            }
        }

        #region HttpRequestAsync オーバロード
        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        public void HttpRequestAsync(string url, string method)
        {
            HttpRequestAsync(url, method, default(DateTime), null);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="callback">Streamを処理するコールバックメソッド</param>
        public void HttpRequestAsync(string url, string method, Action<Stream> callback)
        {
            HttpRequestAsync(url, method, default(DateTime), null, callback);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        public void HttpRequestAsync(string url, string method, DateTime since)
        {
            HttpRequestAsync(url, method, since, null, null);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="callback">Streamを処理するコールバックメソッド</param>
        public void HttpRequestAsync(string url, string method, DateTime since, Action<Stream> callback)
        {
            HttpRequestAsync(url, method, since, null, callback);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="postData">POSTで送信するデータ</param>
        public void HttpRequestAsync(string url, string method, IDictionary<string, string> postData)
        {
            HttpRequestAsync(url, method, default(DateTime), postData, null);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="callback">Streamを処理するコールバックメソッド</param>
        public void HttpRequestAsync(string url, string method, IDictionary<string, string> postData, Action<Stream> callback)
        {
            HttpRequestAsync(url, method, default(DateTime), postData, callback);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="callback">AsyncCallback デリゲート。 </param>
        public void HttpRequestAsync(string url, string method, DateTime since, IDictionary<string, string> postData, Action<Stream> callback)
        {
            HttpRequestAsync(url, method, since, postData, Encoding.Default, callback);
        }

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="enc">文字エンコーディング</param>
        /// <param name="callback">Streamを処理するメソッド</param>
        public void HttpRequestAsync(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, Action<Stream> callback)
        {
            HttpRequestAsync(url, method, since, postData, Encoding.Default, null, callback);
        }
        #endregion

        protected class QueryParameter : IComparable
        {
            public QueryParameter(string key, string value)
            {
                this.Key = key;
                this.Value = value;
            }

            public string Key
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
                return this.Key != qp.Key ? this.Key.CompareTo(qp.Key) : this.Value.CompareTo(qp.Value);
            }

            #endregion
        }
    }
}
