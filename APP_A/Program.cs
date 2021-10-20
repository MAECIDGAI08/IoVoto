using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace AppA
{
    public class Program
    {
        public static void Main(string[] args)
        {   //eredita i servizi dallo startup
            CreateWebHostBuilder(args).Build().Run();
        }


        //public static int Main(string[] args)
        //{
        //    //CreateWebHostBuilder(args).Build().Run();
        //    Log.Logger = new LoggerConfiguration()
        //        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        //        .Enrich.FromLogContext()
        //        .WriteTo.Console()
        //        .CreateLogger();

        //    try
        //    {
        //        Log.Information("Starting web host");
        //        CreateWebHostBuilder(args).Build().Run();
        //        return 0;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Fatal(ex, "Host terminated unexpectedly");
        //        return 1;
        //    }
        //    finally
        //    {
        //        Log.CloseAndFlush();
        //    }




        //}

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
             //.UseSerilog() // <-- Add this line
             .UseStartup<Startup>();
    }
}


