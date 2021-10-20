using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace AppB
{
    public class Program
    {
        public static void Main(string[] args)
        {   //eredita i servizi dallo startup
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
