/*
  Copyright (c) 2009 NekoVampire / Deflis

  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

    1. The origin of this software must not be misrepresented; you must not
    claim that you wrote the original software. If you use this software
    in a product, an acknowledgment in the product documentation would be
    appreciated but is not required.

    2. Altered source versions must be plainly marked as such, and must not be
    misrepresented as being the original software.

    3. This notice may not be removed or altered from any source
    distribution.

  本ソフトウェアは「現状のまま」で、明示であるか暗黙であるかを問わず、
  何らの保証もなく提供されます。 本ソフトウェアの使用によって生じる
  いかなる損害についても、作者は一切の責任を負わないものとします。

  以下の制限に従う限り、商用アプリケーションを含めて、本ソフトウェアを
  任意の目的に使用し、自由に改変して再頒布することをすべての人に許可します。

    1. 本ソフトウェアの出自について虚偽の表示をしてはなりません。
    あなたがオリジナルのソフトウェアを作成したと主張してはなりません。
    あなたが本ソフトウェアを製品内で使用する場合、製品の文書に謝辞を入れていただければ
    幸いですが、必須ではありません。
 
    2. ソースを変更した場合は、そのことを明示しなければなりません。
    オリジナルのソフトウェアであるという虚偽の表示をしてはなりません。

    3. ソースの頒布物から、この表示を削除したり、表示の内容を変更したりしてはなりません。 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;

namespace NekoVampire.Web
{
    /// <summary>
    /// HttpWebRequest ラッパー
    /// </summary>
    public class Request
    {
        /// <summary>NetworkCredentialオブジェクト</summary>
        public NetworkCredential Credentials;

        /// <summary>
        /// Requestを初期化する
        /// </summary>
        public Request()
        {
            Credentials = new NetworkCredential();
        }

        /// <summary>
        /// Requestを初期化する
        /// </summary>
        /// <param name="userName">ユーザーID</param>
        /// <param name="password">パスワード</param>
        public Request(string userName, string password)
        {
            Credentials = new NetworkCredential(userName, password);
        }

        /// <summary>ユーザー名</summary>
        public string UserName
        {
            get
            {
                return Credentials.UserName;
            }
            set
            {
                Credentials.UserName = value;
            }
        }

        /// <summary>パスワード</summary>
        public string Password
        {
            get
            {
                return Credentials.Password;
            }
            set
            {
                Credentials.Password = value;
            }
        }

        /// <summary>HTTPリクエストを行います</summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="enc">文字エンコーディング</param>
        /// <param name="headers">HTTPヘッダー</param>
        /// <param name="invoker">Streamを処理するメソッド</param>
        public void HttpRequest(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, IDictionary<string, string> headers, Action<Stream> invoker)
        {
            if ((postData != null) && (postData.Count != 0) && (method != "POST"))
            {
                url = url + "?" + RequestUtility.GetPostData(postData, enc);
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method;
            req.Credentials = Credentials;
            req.IfModifiedSince = since;

            if (headers != null)
                foreach (var header in headers)
                    req.Headers.Add(header.Key, header.Value);

            if ((postData != null) && (postData.Count != 0) && (method == "POST"))
            {
                byte[] data = getEncodedPostData(postData, enc);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;

                req.ServicePoint.Expect100Continue = false;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                }
                catch (WebException)
                {
                    throw;
                }
            }

            try
            {
                using (WebResponse responce = req.GetResponse())
                {
                    if (invoker != null)
                    {
                        using (Stream resStream = responce.GetResponseStream())
                        {
                            invoker(resStream);
                        }
                    }
                }
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

        /// <summary>
        /// 非同期でHTTPリクエストを行います
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTPメソッド</param>
        /// <param name="since">IfModifiedSinceパラメータ</param>
        /// <param name="postData">POSTで送信するデータ</param>
        /// <param name="enc">文字エンコーディング</param>
        /// <param name="headers">HTTPヘッダー</param>
        /// <param name="callback">Streamを処理するメソッド</param>
        public void HttpRequestAsync(string url, string method, DateTime since, IDictionary<string, string> postData, Encoding enc, IDictionary<string, string> headers, Action<Stream> callback)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = method;
            req.Credentials = Credentials;
            req.IfModifiedSince = since;

            if (headers != null)
                foreach (var header in headers)
                    req.Headers.Add(header.Key, header.Value);

            if (postData != null)
            {
                byte[] data = getEncodedPostData(postData, enc);
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;

                req.ServicePoint.Expect100Continue = false;
                try
                {
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }
                }
                catch (WebException)
                {
                    throw;
                }
            }

            object asyncState = new asyncStateData(req, callback);
            req.BeginGetResponse(new AsyncCallback(asyncCallback), asyncState);
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

        /// <summary>
        /// エンコードされたPOSTで送信するデータを返します
        /// </summary>
        /// <param name="postData">POSTされるデータ</param>
        /// <param name="enc">エンコーディング</param>
        /// <returns>エンコードされたデータ</returns>
        private byte[] getEncodedPostData(IDictionary<string, string> postData, Encoding enc)
        {
            return Encoding.ASCII.GetBytes(RequestUtility.GetPostData(postData, enc));
        }

        private void asyncCallback(IAsyncResult ar)
        {
            asyncStateData state = (asyncStateData)ar.AsyncState;
            using (WebResponse responce = state.HttpWebRequest.EndGetResponse(ar))
            {
                if (state.Callback != null)
                {
                    using (Stream resStream = responce.GetResponseStream())
                    {
                        state.Callback(resStream);
                    }
                }
            }
        }

        private class asyncStateData
        {
            public HttpWebRequest HttpWebRequest;
            public Action<Stream> Callback;

            public asyncStateData(HttpWebRequest req, Action<Stream> callback)
            {
                this.HttpWebRequest = req;
                this.Callback = callback;
            }
        }

        // TODO:アップロード関連のメソッドのブラッシュアップ
        #region アップロードメソッド
        public void HttpRequestUpload(string url, IDictionary<string, string> postData, Encoding enc, string PostName, string fileName, string contentType, Action<Stream> invoker)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.Credentials = Credentials;

            if (postData != null)
            {
                using (var post = new PostFileData())
                {
                    foreach (var line in postData)
                    {
                        post.Add(line.Key, line.Value);
                    }
                    using (var stream = new FileStream(fileName, FileMode.Open))
                    {
                        post.Add(PostName, stream, (int)stream.Length, Path.GetFileName(fileName), contentType);
                    }
                    req.ContentType = "multipart/form-data; boundary=" + post.Boundary;

                    var data = post.ToBytes();
                    req.ContentLength = data.Length;

                    try
                    {
                        using (Stream reqStream = req.GetRequestStream())
                        {
                            reqStream.Write(data, 0, data.Length);
                            reqStream.Close();
                        }
                    }
                    catch (WebException)
                    {
                        throw;
                    }
                }
            }

            try
            {
                using (WebResponse responce = req.GetResponse())
                {
                    if (invoker != null)
                    {
                        using (Stream resStream = responce.GetResponseStream())
                        {
                            invoker(resStream);
                        }
                    }
                }
            }
            catch (WebException)
            {
                throw;
            }
        }

        private class PostFileData : IDisposable
        {

            private MemoryStream MemoryStream = new MemoryStream();

            private string _boundary;

            public string Boundary
            {
                get
                {
                    return _boundary;
                }
            }

            public PostFileData()
            {
                _boundary = "--------------------" + Environment.TickCount.ToString();
            }

            public void Add(string name, string value)
            {
                var sb = new StringBuilder();
                sb.Append("--" + Boundary + "\n");
                sb.Append("Content-Disposition: form-data; name=\"" + name + "\"\n\n");
                sb.Append(value + "\n");
                var buf = sb.ToString();
                MemoryStream.Write(Encoding.UTF8.GetBytes(buf), 0, buf.Length);
            }

            public void Add(string name, Stream stream, int size, string file, string contentType)
            {
                var sb = new StringBuilder();
                sb.Append("--" + Boundary + "\n");
                sb.Append("Content-Disposition: form-data; name=\"" + name + "\"; filename=\"" + file + "\"\n");
                sb.Append("Content-Type: " + contentType + "\n");
                var buf = Encoding.UTF8.GetBytes(sb.ToString());

                MemoryStream.Write(buf, 0, buf.Length);

                using (var br = new BinaryReader(stream))
                {
                    MemoryStream.Write(br.ReadBytes(size), 0, size);
                }

                MemoryStream.Write(Encoding.ASCII.GetBytes("\n"), 0, "\n".Length);
            }

            public byte[] ToBytes()
            {
                var termination = "--" + _boundary + "--\n";
                using (var stream = new MemoryStream())
                {
                    stream.Write(Encoding.ASCII.GetBytes(termination), 0, termination.Length);
                    MemoryStream.WriteTo(stream);
                    return stream.ToArray();
                }
            }

            #region IDisposable メンバ

            public void Dispose()
            {
                MemoryStream.Dispose();
            }

            #endregion
        }
        #endregion
    }
}
