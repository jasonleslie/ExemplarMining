using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace Exemplar_Mining
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, config) =>
           {
               config.SetBasePath(hostContext.HostingEnvironment.ContentRootPath);
               Console.WriteLine(hostContext.HostingEnvironment.EnvironmentName);
               config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
               config.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true);
               config.AddEnvironmentVariables();
           })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
