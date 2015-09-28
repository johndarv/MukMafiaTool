using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MukMafiascumWebsite.Startup))]
namespace MukMafiascumWebsite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
