using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using System.Diagnostics;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Extensions.DependencyInjection;
using NewUltimusWeb.Services;
using System.Web.Mvc;
using System.Linq;
using NewUltimusWeb.Models;
using System.Web;

[assembly: OwinStartup(typeof(NewUltimusWeb.Startup))]

namespace NewUltimusWeb
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersAsServices(typeof(Startup).Assembly.GetExportedTypes()
                .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
                .Where(t => typeof(IController).IsAssignableFrom(t) || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));
            services.AddTransient<IBpmAuthenticationManager<IAccount>, UltimusAuthenticationManager>();
            services.AddTransient<IOwinContext, OwinContext>();
        }

        public void Configuration(IAppBuilder app)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            var resolver = new DependencyResolverService(services.BuildServiceProvider());
            DependencyResolver.SetResolver(resolver);

            // add owin security middleware            
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "BpmAppicationCookie",
                LoginPath = new PathString("/Auth/Login"),
                LogoutPath = new PathString("/Auth/Logout")
            });
        }
    }
}
