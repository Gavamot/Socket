using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Configuration
{
    /// <summary>
    /// Настройки для подключение к ASC 3.0 
    /// </summary>
    public class AscConfig
    {
        public int ConnectionPoolSize { get; set; }
        public ConnectionSettings Connect { get; set; }
        public ServiceConfig DeviceService { get; set; }
    }
}
