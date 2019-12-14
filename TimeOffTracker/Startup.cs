using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TimeOffTracker.Startup))]
namespace TimeOffTracker
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
