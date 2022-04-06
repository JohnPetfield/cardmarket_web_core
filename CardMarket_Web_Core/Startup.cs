using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CardMarket_Web_Core
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //https://thomaslevesque.com/2018/04/17/hosting-an-asp-net-core-2-application-on-a-raspberry-pi/
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            // Patch path base with forwarded path
            //https://thomaslevesque.com/2018/04/17/hosting-an-asp-net-core-2-application-on-a-raspberry-pi/

            /*
            app.Use(async (context, next) =>
            {
                var forwardedPath = context.Request.Headers["X-Forwarded-Path"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedPath))
                {
                    context.Request.PathBase = forwardedPath;
                    Console.WriteLine($"forwarded path: {forwardedPath}");
                }

                await next.Invoke();
            });
            */

            /* the retrieving  the x-forwarded-path didn't work from the above article*/

            /// https://stackoverflow.com/questions/69947346/multiple-net-core-apps-same-domain-on-nginx
            // this stackoverflow has just hardcoded basically, so this is stored
            // in nginx config and here, it works, but not clear what's wrong with the 
            // first approach

            //THIS WORKS
            //app.UsePathBase("/cardmarket");

            //THIS ALSO WORKS and is better than app.UsePathBase
            app.UsePathBase(Configuration.GetValue<string>("NGINX_location_subdirectory"));



            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-5.0
                /// 
                /*
                endpoints.MapControllerRoute(name: "CardMarket",
                pattern: "findsellers",
                defaults: new { controller = "CardMarket", action = "Index" });
                */
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=CardMarket}/{action=Index}/{id?}");
            });
        }
    }
}
