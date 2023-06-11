using System;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Intaria.Models;
using Stripe;

namespace Intaria
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

            services.Configure<GoogleConfigModel>(Configuration.GetSection(GoogleConfigModel.GoogleConfig));

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10); 
                options.Cookie.IsEssential = true;
                options.Cookie.Name = ".intaria.Session";

            });

            



            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();

            services.Configure<StripeSettings>(Configuration.GetSection("Stripe"));
            services.Configure<PaymodelView>(Configuration.GetSection("PayStripe"));

            services.AddControllersWithViews();
            services.AddRazorPages()
                .AddRazorRuntimeCompilation();


            services.AddDistributedMemoryCache();

            services.AddHttpContextAccessor();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StripeConfiguration.ApiKey = "sk_test_51IS4HRItUNYMGyQ5MvhaIVh1lho78ZInL8GeWiIPONQBuoi6ZxcZSx14CKrnYhdEzfbAGisNnLsCw8emmFYImylA00v7ZBqsGl";


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();


            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "articulo",
                    pattern: "articulo/{id}",
                    defaults: new { controller = "Web", action = "Articulo"});

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Web}/{action=Index}/{id?}");
                

            });


        }
    }
}
