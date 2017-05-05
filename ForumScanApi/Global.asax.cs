#pragma warning disable SA1649 // File name must match first type name

namespace ForumScanApi
{
    using System.Web.Http;

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

#pragma warning restore SA1649