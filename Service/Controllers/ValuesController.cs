using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Service.DataClasses;
using Service.Core;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Service.Controllers
{
    //[Route("api/[controller]")]
    public class ValuesController : Controller
    {
        protected readonly Config config;
        protected readonly ILogger logger;
        public ValuesController(IOptions<Config> config, ILogger<ValuesController> logger)
        {
            this.config = config.Value;
            this.logger = logger;
        }

        [Route("[controller]/Brigades")]
        public DevicesListItem[] Get()
        {
            var message = Message.CreateAscGetBrigadesInfoMessage();
            var res = Startup.Pool.Get<DevicesListItem[]>(message);
            return res;
        }
    }
}
