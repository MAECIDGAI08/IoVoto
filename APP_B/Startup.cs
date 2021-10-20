
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using AppB.Models;

namespace AppB
{
    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            StaticConfig = configuration;
        }
        public static IConfiguration StaticConfig { get; private set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //istanzio i servizi che verranno utilizzati dall'app
            //services.AddSession(Options => Options.IdleTimeout);

            services.Configure<Parameters>(_configuration.GetSection("Parameters"));


            //1 - aggiungo i servizi di localizzazione (inject IStringLocalizer)
            services.AddLocalization(options =>
            {
                //definisco la cartella che conterrà i file resx;
                options.ResourcesPath = "Resources";
            });

            //2 - definisco le culture supportate e quella di default (RequestLocalizationMiddleware)
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var defaultCulture = "it-IT";
                // culture supportate dall'applicazione
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo(defaultCulture),
                    new CultureInfo("en"),
                };
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                var rcpCookie = options.RequestCultureProviders.FirstOrDefault(x => x is Microsoft.AspNetCore.Localization.CookieRequestCultureProvider);
                //cancello tutti i provider definiti di default
                options.RequestCultureProviders.Clear();
                //aggiungo il provider salvato precedentemente
                if (rcpCookie != null)
                    options.RequestCultureProviders.Add(rcpCookie);

            });
                 
            // NEW per sessioni on page
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                //options.CheckConsentNeeded = context => true;
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddDistributedMemoryCache();          
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AppB.Session";

                // per testing della sessione
                // options.IdleTimeout = TimeSpan.FromSeconds(5);
                // Set session timeout value 30 minutes
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                //options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; // cookie sessione essenziale
            });
            // FINE NEW per sessioni on page

            


            // aggiunta servizio autorizzazione
            services.AddAuthorization();
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddMvc()
               .AddRazorPagesOptions(options =>
               {
                   options.AllowAreas = true;
                   // pagine in cui entro pubblicamente
                   options.Conventions.AllowAnonymousToPage("/Home/NoteLegali");
                   options.Conventions.AllowAnonymousToPage("/Home/CookiePolicy");
                   options.Conventions.AllowAnonymousToPage("/Home/Privacy");
                  
                   // pagine in cui entro se sono autorizzato 
                   options.Conventions.AuthorizePage("/Home/Index");
                   options.Conventions.AuthorizePage("/Home/ConfermaVoto");
                   options.Conventions.AuthorizePage("/Home/ListeElettorali");
                   options.Conventions.AuthorizePage("/Home/Sessione");
                   options.Conventions.AuthorizePage("/Home/Logout");
                   //options.Conventions.AuthorizeFolder("/Private");
                   //options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
                   //options.Conventions.AllowAnonymousToFolder("/Private/PublicPages");
               })
               


                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                 //3 - add localization service per le view (inject IViewLocalizer)

                 .AddViewLocalization(
                     LanguageViewLocationExpanderFormat.Suffix,
                     opts => { opts.ResourcesPath = "Resources"; })
                 .AddDataAnnotationsLocalization();
            // FRA
            

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var path = Directory.GetCurrentDirectory();
            loggerFactory.AddFile($"{path}\\Logs\\Logs_ApplicationB.txt");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            
            // appA & appB
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
            
            // appA & appB
            app.UseHttpsRedirection();

            // appA & appB
            app.UseStaticFiles();

            // appA & appB
            // app.UseCookiePolicy();

            // appA & appB
            app.UseSession();

            //4 - add localization, prima di app.UseMvc
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
