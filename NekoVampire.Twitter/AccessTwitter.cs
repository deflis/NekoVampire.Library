using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NekoVampire.Web.OAuth;
using System.Security;

namespace NekoVampire.TwitterAccess
{
    /// <summary>Twitterへのアクセスを提供する</summary>
    public class AccessTwitter : OAuthAccess
    {
        private const string API = "http://twitter.com/";
        private const string APIv1 = "http://api.twitter.com/1/";

        #region コンストラクタ
        public AccessTwitter(string consumerKey, string consumerSecret)
            : base(consumerKey, consumerSecret, API + "oauth/request_token", API + "oauth/access_token", API + "oauth/authorize")
        { }

        public AccessTwitter(string consumerKey, string consumerSecret, string token, SecureString tokenSecret)
            : base(consumerKey, consumerSecret, API + "oauth/request_token", API + "oauth/access_token", API + "oauth/authorize", token, tokenSecret)
        { }
        #endregion
        
        #region GetTimeLine
        
        /// <summary>
        /// 自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。
        /// </summary>
        /// <param name="invoker"></param>
        public void GetTimeLine(Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/home_timeline.xml";

            try
            {
                HttpRequest(url,"GET",invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。
        /// </summary>
        /// <param name="page">ページ番号</param>
        public void GetTimeLine(int page, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/home_timeline.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 自分の friend の過去24時間以内に update されたステータスから最大count件を取得する。
        /// </summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数(200まで)</param>
        public void GetTimeLine(int page, int count, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/home_timeline.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。
        /// </summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        public void GetTimeLine(int page, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/home_timeline.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 自分の friend の過去24時間以内に update されたステータスからcount(=200まで)件を取得する。
        /// </summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        /// <param name="count">取得件数</param>
        public void GetTimeLine(int page, int count, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/home_timeline.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region GetReplies
        /// <summary>自分に対する返信(冒頭が @ユーザ名 で始まるステータス)の一覧を取得する (最大20件)</summary>
        public void GetReplies(Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/mentions.xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分に対する返信(冒頭が @ユーザ名 で始まるステータス)の一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        public void GetReplies(int page, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/mentions.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分に対する返信(冒頭が @ユーザ名 で始まるステータス)の一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数</param>
        public void GetReplies(int page, int count, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/mentions.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分に対する返信(冒頭が @ユーザ名 で始まるステータス)の一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        public void GetReplies(int page, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/mentions.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分に対する返信(冒頭が @ユーザ名 で始まるステータス)の一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        /// <param name="count">取得件数</param>
        public void GetReplies(int page, int count, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/mentions.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region GetUserTimeLine
        /// <summary>自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。</summary>
        public void GetUserTimeLine(string user, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/user_timeline/" + user + ".xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。</summary>
        /// <param name="page">ページ番号</param>
        public void GetUserTimeLine(string user, int page, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/user_timeline/" + user + ".xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分の friend の過去24時間以内に update されたステータスから最大count(=200まで)件を取得する。</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数</param>
        public void GetUserTimeLine(string user, int page, int count, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/user_timeline/" + user + ".xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分の friend の過去24時間以内に update されたステータスから最大20件を取得する。</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        public void GetUserTimeLine(string user, int page, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/user_timeline/" + user + ".xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分の friend の過去24時間以内に update されたステータスからcount(=200まで)件を取得する。</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        /// <param name="count">取得件数</param>
        public void GetUserTimeLine(string user, int page, int count, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/user_timeline/" + user + ".xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion


        /// <summary>
        /// 自分のステータスを更新(update)する。
        /// </summary>
        /// <param name="status">ステータス</param>
        public void SendUpdate(string status, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/update.xml";
            Dictionary<string,string> postData = new Dictionary<string,string>();
            postData.Add("status", status);

            try
            {
                HttpRequest(url, "POST", postData, invoker);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>自分のステータスを更新(update)する。</summary>
        /// <param name="status">ステータス</param>
        public void SendUpdate(string status, string inReplyToStatusId, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/update.xml";
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("status", status);
            postData.Add("in_reply_to_status_id", inReplyToStatusId);

            try
            {
                HttpRequest(url, "POST", postData, invoker);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>ステータスを削除する。</summary>
        /// <param name="id">ステータスID</param>
        public void DestroyStatus(string id, Action<Stream> invoker)
        {
            string url = APIv1 + "statuses/destroy/" + id + ".xml";

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="invoker"></param>
        public void CreateFav(string id, Action<Stream> invoker)
        {
            string url = APIv1 + "favorites/create/" + id + ".xml";

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="invoker"></param>
        public void DestroyFav(string id, Action<Stream> invoker)
        {
            string url = APIv1 + "favorites/destroy/" + id + ".xml";

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ShowUser(string user, Action<Stream> invoker)
        {
            string url = APIv1 + "users/show/" + user + ".xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void CreateFriend(string user, Action<Stream> invoker)
        {
            string url = APIv1 + "friendships/create/" + user + ".xml";

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DestroyFriend(string user, Action<Stream> invoker)
        {
            string url = APIv1 + "friendships/destroy/" + user + ".xml";

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ExistFriend(string user_a, string user_b, Action<Stream> invoker)
        {
            string url = APIv1 + "friendships/exists.xml"
                + "?user_a=" + user_a
                + "&user_b=" + user_b;

            try
            {
                HttpRequest(url, "POST", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetFriends(Action<Stream> invoker)
        {
            string url = APIv1 + "friends/ids.xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetFriends(string id, Action<Stream> invoker)
        {
            string url = APIv1 + "friends/ids/" + id + ".xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetFollowers(Action<Stream> invoker)
        {
            string url = APIv1 + "followers/ids.xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void GetFollowers(string id, Action<Stream> invoker)
        {
            string url = APIv1 + "followers/ids/" + id + ".xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region GetDirectMessage
        /// <summary>自分宛てのダイレクトメッセージの一覧を取得する (最大20件)</summary>
        public void GetDirectMessage(Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages.xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分宛てのダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        public void GetDirectMessage(int page, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分宛てのダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数</param>
        public void GetDirectMessage(int page, int count, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分宛てのダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        public void GetDirectMessage(int page, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分宛てのダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数</param>
        /// <param name="since">日時</param>
        public void GetDirectMessage(int page, int count, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region GetDirectMessageSent
        /// <summary>自分が送信したダイレクトメッセージの一覧を取得する (最大20件)</summary>
        public void GetDirectMessageSent(Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/sent.xml";

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分が送信したダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        public void GetDirectMessageSent(int page, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/sent.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分が送信したダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="count">取得件数</param>
        public void GetDirectMessageSent(int page, int count, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/sent.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分が送信したダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        public void GetDirectMessageSent(int page, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/sent.xml?page=" + page;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>自分が送信したダイレクトメッセージの一覧を取得する (最大20件)</summary>
        /// <param name="page">ページ番号</param>
        /// <param name="since">日時</param>
        /// <param name="count">取得件数</param>
        public void GetDirectMessageSent(int page, int count, DateTime since, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/sent.xml?page=" + page + "&count=" + count;

            try
            {
                HttpRequest(url, "GET", since, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>ダイレクトメッセージを送信する。</summary>
        /// <param name="status">ステータス</param>
        public void SendDM(string user, string text, Action<Stream> invoker)
        {
            string url = APIv1 + "direct_messages/new.xml";
            Dictionary<string, string> postData = new Dictionary<string, string>();
            postData.Add("user", user);
            postData.Add("text", text);

            try
            {
                HttpRequest(url, "POST", postData, invoker);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
