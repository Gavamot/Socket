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
            return res;
        }

        [Route("Test")] // Brigade/DeviceList
        public string Test()
        {
            return "Hello";
        }
    }
}
