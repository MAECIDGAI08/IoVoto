using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
using Utility;

using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.HttpOverrides;

namespace AppA
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			StaticConfig = configuration;
        }
		public static IConfiguration StaticConfig { get; private set; }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            #region  INTEGRAZIONE SPID
            //START INTEGRAZIONE SPID
            //services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseSqlServer(
            //    Configuration.GetConnectionString("DefaultConnection")));

            //services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI().AddDefaultTokenProviders();
            /*services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();*/
            //END INTEGRAZIONE SPID
            #endregion
            #region  INTEGRAZIONE SPID
            //services.AddIdentity<ApplicationUser, Microsoft.AspNet.Identity.EntityFramework.IdentityRole>(
            //options => {
            //        options.SignIn.RequireConfirmedAccount = true;
            //        options.Lockout.AllowedForNewUsers = true;
            //        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(9000);
            //        options.Lockout.MaxFailedAccessAttempts = 5;
            //    }).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultUI().AddDefaultTokenProviders();
            #endregion
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => false;
                //options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
               
            });
            
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = ".AppA.Session";

                // per testing della sessione
                //options.IdleTimeout = TimeSpan.FromSeconds(5);

                // TEMPO SESSIONE 
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                // options.Cookie.HttpOnly = true;
                // Make the session cookie essential
                options.Cookie.IsEssential = true;
            });

            // aggiunta servizio autorizzazione
            services.AddAuthorization();

            // IP address
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddMvc()
                 .AddRazorPagesOptions(options =>
                 {
                     options.AllowAreas = true;
                     // pagine in cui entro pubblicamente
                     options.Conventions.AllowAnonymousToPage("/Home/NoteLegali");
                     options.Conventions.AllowAnonymousToPage("/Home/CookiePolicy");
                     options.Conventions.AllowAnonymousToPage("/Home/Privacy");
                     options.Conventions.AllowAnonymousToPage("/Home/Index");

                     // pagine in cui entro se sono autorizzato 
                     options.Conventions.AuthorizePage("/Home/CodiceElettore");
                     options.Conventions.AuthorizePage("/Home/RicevutadiVoto");
                     options.Conventions.AuthorizePage("/Home/VaiAlVoto");
                     options.Conventions.AuthorizePage("/Home/Sessione");
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

        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //prende la directory del file di testo contenente i log
            var path = Directory.GetCurrentDirectory();
                        
            loggerFactory.AddFile($"{path}\\Logs\\Logs_Application_A.txt");
              
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
            
            app.UseStaticFiles();
            app.UseSession();

         

            //4 - add localization, prima di app.UseMvc
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(options.Value);

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                    );
            });
            
        }
    }
}
