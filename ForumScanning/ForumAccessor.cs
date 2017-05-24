namespace MukMafiaTool.ForumScanning
{
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
    using MukMafiaTool.Common;

    public class ForumAccessor
    {
        private HttpResponseHeaders signedInHeaders;
        private string forumUsername;
        private string forumPassword;

        public ForumAccessor()
        {
            this.forumUsername = ConfigurationManager.AppSettings["ForumUsername"];
            this.forumPassword = ConfigurationManager.AppSettings["ForumPassword"];
            this.signedInHeaders = this.SignIn();
        }

        public string RetrievePageHtml(int pageNumber)
        {
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.UseCookies = false;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://www.rllmukforum.com/");

                    var url = string.Format("{0}/page-{1}", ConfigurationManager.AppSettings["ThreadBaseAddress"], pageNumber);

                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);

                    foreach (var cookieHeader in this.signedInHeaders.Where(h => string.Equals(h.Key, "Set-Cookie")))
                    {
                        message.Headers.Add("Cookie", cookieHeader.Value);
                    }

                    var response = client.Send(message);

                    var currentUri = response.RequestMessage.RequestUri.AbsoluteUri;
                    var index = currentUri.IndexOf("page", StringComparison.OrdinalIgnoreCase);

                    var currentActualPageNumber = index == -1 ? 1 : int.Parse(currentUri.Substring(index + 5, currentUri.Length - (index + 5)));

                    if (pageNumber != currentActualPageNumber)
                    {
                        return string.Empty;
                    }

                    return response.GetContentAsString();
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
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, "index.php?/login");

            message.Content = content;

            AddHeaders(message);

            return message;
        }

        private static void AddHeaders(HttpRequestMessage message)
        {
            message.Headers.Host = "www.rllmukforum.com";
            message.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            message.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            message.Headers.Add("Accept-Language", "en-US,en;q=0.8");
            message.Headers.Add("Cache-Control", "max-age=0");
            message.Headers.Add("Connection", "keep-alive");
            message.Headers.Add("Origin", "https://www.rllmukforum.com");
            message.Headers.Add("Referer", "https://www.rllmukforum.com/?_fromLogin=1&_fromLogout=1");
            message.Headers.Add("Upgrade-Insecure-Requests", "1");
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/57.0.2987.98 Safari/537.36");
            message.Headers.Add("Cookie", "ips4_IPSSessionFront=kjhgc0i3396q0mj1i32e6mtpd3; ips4_ipsTimezone=Europe/London; ips4_hasJS=true");
        }

        private FormUrlEncodedContent CreateContentToSignIn()
        {
            var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("csrfKey", "af5213a6610a082fe44d6300e96c5c5c"),
                    new KeyValuePair<string, string>("auth", this.forumUsername),
                    new KeyValuePair<string, string>("password", this.forumPassword),
                    new KeyValuePair<string, string>("login__standard_submitted", "1"),
                    new KeyValuePair<string, string>("remember_me", "0"),
                    new KeyValuePair<string, string>("signin_anonymous", "0"),
                });

            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            return content;
        }

        private HttpResponseHeaders SignIn()
        {
            using (var handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;

                using (HttpClient client = new HttpClient(handler))
                {
                    client.BaseAddress = new Uri("https://www.rllmukforum.com");

                    var content = this.CreateContentToSignIn();

                    HttpRequestMessage message = CreateMessageToSignIn(content);

                    var response = client.Send(message);

                    var responseContent = DecodeHttpContentToString(response);

                    if (response.StatusCode != HttpStatusCode.RedirectMethod)
                    {
                        //// TODO: Log the fact that we couldn't sign in to the forum

                        var forumCookie = ConfigurationManager.AppSettings["ForumCookie"];

                        var fakeResponse = new HttpResponseMessage();
                        fakeResponse.Headers.Add("Set-Cookie", forumCookie);

                        response = fakeResponse;
                    }

                    return response.Headers;
                }
            }
        }
    }
}