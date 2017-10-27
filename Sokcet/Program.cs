using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Sokcet;

namespace Test
{
    class Program
    {

        static void Main(string[] args)
        {
            int countClients = 2;
            var cliensts = new AsynchronousClient[countClients];
            for (int i = 0; i < countClients; i++)
            {
                var c = new AsynchronousClient();    
                c.StartClient();
                cliensts[i] = c;
            }

            while (true)
            {
                Task.Factory.StartNew(() =>
                {
                    var pool = new ObjectPool<AsynchronousClient>(cliensts);
                    var client = pool.GetObject();
                    var b = client.Reqvest(Message.CreateGiveIve50ArchiveCodesInfoMessage(client));
                    var res = Encoding.UTF8.GetString(b.ToArray());
                    Debug.WriteLine(res);
                });
            }

            Console.ReadKey();
        }
    }
}
