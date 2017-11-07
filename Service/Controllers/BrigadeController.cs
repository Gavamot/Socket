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
        protected readonly AscPool pool;
        public BrigadeController(ILogger<BrigadeController> logger, AscPool pool)
        {
            this.logger = logger;
            this.pool = pool;
        }

        [Route("[controller]/Brigades")]
        public DevicesListItem[] DeviceList()
        {
            var message = Message.CreateAscGetBrigadesInfoMessage();
            var res = pool.Get<DevicesListItem[]>(message);
            return res;
        }
    }
}
