using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(iNQUIRE.Startup))]
namespace iNQUIRE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
