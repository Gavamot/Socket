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
        //private static void Test()
        //{
        //    var loggerFactory = new LoggerFactory();
        //    loggerFactory.AddFile("Logs/mylog-{Date}.txt");
        //    var log = loggerFactory.CreateLogger("123");
        //    var pool = new AscPool(AscConfig.GetFakeConfig(), log);

        //    for (int i = 0; i < 80; i++)
        //    {
        //        var thread = new Thread(() =>
        //        {
        //            var msg = Message.CreateAscGetBrigadesInfoMessage();
        //            int ii = 1;
        //            while (true)
        //            {
        //                var res = pool.Get<DevicesListItem[]>(msg);
        //                //Console.WriteLine(res == null
        //                //    ? $"{Thread.CurrentThread.Name} - Не удалось выполнить запрос"
        //                //    : $"{Thread.CurrentThread.Name} - {ii++}");
        //            }
        //        })
        //        {
        //            Name = $"thread={i}"
        //        };
        //        thread.Start();
        //    }
        //    Console.ReadKey();
        //}

        public static void Main(string[] args)
        {
            //Test();

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
