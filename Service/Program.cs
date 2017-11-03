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
        private static void Test()
        {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddFile("Logs/mylog-{Date}.txt");
            var log = loggerFactory.CreateLogger("123");

            var pool = new AscPool(10, AscConfig.GetFakeConfig(), log);
            for (int i = 0; i < 20; i++)
            {
                var thread = new Thread(() =>
                {
                    var msg = Message.CreateAscGetBrigadesInfoMessage();
                    int ii = 1;
                    while (true)
                    {
                        try
                        {
                            var res = pool.Get<DevicesListItem[]>(msg);
                            Console.WriteLine($"{ii++}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"{Thread.CurrentThread.Name} {e.Message}");
                            log.LogError($"{Thread.CurrentThread.Name} {e.Message}");
                        }
                    }
                })
                {
                    Name = $"thread={i}"
                };
                thread.Start();
            }
            Console.ReadKey();
        }

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
