using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service.Core;
using Service.DataClasses;
using System.Threading;

namespace Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true)
               .Build();

            var port = config["port"];

            var host = WebHost.CreateDefaultBuilder(args)
               .UseStartup<Startup>()
               .UseUrls($"http://*:{port}")
               .UseIISIntegration()
               .Build();

            host.Run();
        }  
    }
}
