using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace SwaMiddlewareDemo
{
    public class Program
    {
        //dotnet run [listening url]
        //dotnet run http://localhost:5050
        public static void Main(string[] args)
        {
            var hostBuilder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>();

            if (args.Length > 0)
            {
                hostBuilder.UseUrls(args[0]);
            }

            hostBuilder.Build().Run();
        }
    }
}
