using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace WebService.Controllers
{
    public class BaseController<T> : Controller
    {
        protected readonly Config config;
        protected readonly ILogger logger;
        public BaseController(IOptions<Config> config, ILogger<T> logger)
        {
            this.config = config.Value;
            this.logger = logger;

        }
    }
}
