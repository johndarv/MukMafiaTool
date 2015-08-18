using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MukMafiaTool.InternalForumScanner
{
    public static class Extensions
    {
        public static HttpResponseMessage Send(this HttpClient client, HttpRequestMessage message)
        {
            var task = client.SendAsync(message);
            task.Wait();
            return task.Result;
        }

        public static string GetContentAsString(this HttpResponseMessage message)
        {
            var task = message.Content.ReadAsStringAsync();
            task.Wait();
            return task.Result;
        }
    }
}