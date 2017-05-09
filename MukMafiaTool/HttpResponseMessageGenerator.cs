namespace MukMafiaTool
{
    using System.Net;
    using System.Net.Http;

    public static class HttpResponseMessageGenerator
    {
        public static HttpResponseMessage GenerateOKMessage()
        {
            var message = new HttpResponseMessage();
            message.StatusCode = HttpStatusCode.OK;
            return message;
        }
    }
}