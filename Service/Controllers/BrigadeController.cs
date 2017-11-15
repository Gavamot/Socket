using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.DataClasses;
using Service.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Service.Services;

namespace Service.Controllers
{
    //[Route("api/[controller]")]
    public class BrigadeController : Controller
    {
        //protected readonly Config config;
        protected readonly ILogger logger;
        protected readonly BrigadeService bs = Startup.Bs;

        public BrigadeController(ILogger<BrigadeController> logger)//, BrigadeService bs)
        {
            this.logger = logger;
            //this.bs = bs;
        }

        [Route("[controller]/DeviceList")] // Brigade/DeviceList
        public ResponseMessage<DevicesListItem[]> DeviceList()
        {
            var res = bs.GetDevicesList();
            res.Data = GenerateGForTest(res.Data, 200);
            return res;
        }

        private DevicesListItem[] GenerateGForTest(DevicesListItem[] items, int gCount)
        {
            var res = new List<DevicesListItem>();
            res.AddRange(items);
            var rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < gCount; i++)
                res.Add(GetGItem(i, rnd));
            return res.ToArray();
        }

        private DevicesListItem GetGItem(int number, Random rnd)
        {
            var res = new DevicesListItem();
            res.Position = new Position
            {
                Brigade = "b" + number,
                Cluster = "c" + number,
                Field = "f" + number,
                Well = "w" + number,
                Work = "work" + number
            };
            res.Archive = new Archive
            {
                Load = rnd.Next(1400),
                Size = rnd.Next(50000)
            };
            res.Device = new Device
            {
                Model = (ushort) rnd.Next(255),
                Number = (ushort) rnd.Next(255),
                Release = (ushort) rnd.Next(255),
                Serial = (ushort) rnd.Next(255),
                Type = rnd.Next(255).ToString(),
                Version = rnd.Next(255).ToString()
            };
            res.Info = new Info
            {
                Active = rnd.Next(100) > 50,
                Available = rnd.Next(100) > 50,
                Video = rnd.Next(100) > 50,
                Time = DateTime.Now.ToString(),
                Location = new Location()
                {
                    X = rnd.Next(100),
                    Y = rnd.Next(100)
                }
            };
            return res;
        }

        [Route("Test")] // Brigade/DeviceList
        public string Test()
        {
            return "Hello";
        }
    }
}
