using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using WebService.DataClasses;

namespace WebService.Controllers
{
    [Produces("application/json")]
    [Route("api/Device")]
    public class DeviceController : BaseController<DeviceController>
    {
        public DeviceController(IOptions<Config> config, ILogger<DeviceController> logger) 
            : base(config, logger) { }

        [HttpGet]
        public IEnumerable<DevicesListItem> Get()
        {
            return asc.Reqvest<DevicesListItem[]>(Message.CreateAscGetBrigadesInfoMessage());
        }
    }
}