using Presentation.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using MCSMqttBus.Producer;
using System;
namespace Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Information("Program is preparing...");
            Console.WriteLine("Program is preparing...");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
