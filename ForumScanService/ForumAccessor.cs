using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using MukMafiaTool.Common;

namespace MukMafiaTool.ForumScanService
{
    public class ForumAccessor
    {
        HttpResponseHeaders _signedInHeaders;
        string _forumUsername;
        string _forumPassword;

        public ForumAccessor()
        {
            _forumUsername = ConfigurationManager.AppSettings["ForumUsername"];
            _forumPassword = ConfigurationManager.AppSettings["ForumPassword"];
            _signedInHeaders = SignIn();
        }

        public string RetrievePageHtml(int pageNumber)
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.UseCookies = false;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("http://www.rllmukforum.com/");

                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, string.Format("index.php?/topic/287562-/page-{0}", pageNumber));

                    var requestUri = message.RequestUri;

                    foreach (var cookieHeader in _signedInHeaders.Where(h => string.Equals(h.Key, "Set-Cookie")))
                    {
                        message.Headers.Add("Cookie", cookieHeader.Value);
                    }

                    var response = client.Send(message);

                    var currentUri = response.RequestMessage.RequestUri.AbsoluteUri;
                    var index = currentUri.IndexOf("page", StringComparison.OrdinalIgnoreCase);
                    var currentActualPageNumber = int.Parse(currentUri.Substring(index + 5, currentUri.Length - (index + 5)));

                    if (pageNumber != currentActualPageNumber)
                    {
                        return string.Empty;
                    }

                    return response.GetContentAsString();
                }
            }
        }

        private HttpResponseHeaders SignIn()
        {
            using (var handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("http://www.rllmukforum.com/"); //?app=core&module=global&section=login&do=process

                    var content = CreateContentToSignIn();

                    HttpRequestMessage message = CreateMessageToSignIn(content);

                    var response = client.Send(message);

                    var responseContent = DecodeHttpContentToString(response);

                    if (response.StatusCode != HttpStatusCode.Redirect)
                    {
                        throw new Exception("Could not sign in to the forum. Maybe username / password incorrect or not set?");
                    }

                    return response.Headers;
                }
            }
        }

        private static string DecodeHttpContentToString(HttpResponseMessage response)
        {
            // Read in the input stream, then decompress in to the outputstream.
            // Doing this asynronously, but not really required at this point
            // since we end up waiting on it right after this.
            MemoryStream outputStream = new MemoryStream();
            var task2 = response.Content.ReadAsStreamAsync().ContinueWith(t =>
            {
                Stream inputStream = t.Result;
                var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);

                gzipStream.CopyTo(outputStream);
                gzipStream.Dispose();

                outputStream.Seek(0, SeekOrigin.Begin);
            });

            task2.Wait();
            var responseContent = Encoding.ASCII.GetString(outputStream.ToArray());
            return responseContent;
        }

        private static HttpRequestMessage CreateMessageToSignIn(FormUrlEncodedContent content)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "index.php?app=core&module=global&section=login&do=process");

            message.Content = content;

            AddHeaders(message);

            message.Headers.Add("Cookie", "forums_session_id=b59e0bc00be251aca2b5817e0bd4dc67; _ga=GA1.2.532253799.1439741610; forums_member_id=0; forums_pass_hash=0; ipsconnect_ff9a274e1b4874bb770523ed4e59f407=0; forums_coppa=0; forums_member_id=5212; forums_pass_hash=123b90b4bdd2267e7e47b1f2666f7d12; forums_modtids=%2C; _gat=1; forums_rteStatus=rte");

            return message;
        }

        private static void AddHeaders(HttpRequestMessage message)
        {
            message.Headers.Host = "www.rllmukforum.com";
            message.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            message.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            message.Headers.Add("Accept-Encoding", "gzip, deflate");
            message.Headers.Add("Connection", "keep-alive");
        }

        private FormUrlEncodedContent CreateContentToSignIn()
        {
            var content = new FormUrlEncodedContent(new[] 
                {
                    new KeyValuePair<string, string>("auth_key", "880ea6a14ea49e853634fbdc5015a024"),
                    new KeyValuePair<string, string>("ips_username", _forumUsername),
                    new KeyValuePair<string, string>("ips_password", _forumPassword),
                });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
        }
    }
}