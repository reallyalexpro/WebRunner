using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging((context, logging) => {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    string basePath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    config.SetBasePath(basePath).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
