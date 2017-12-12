using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Configuration
{
    public class ConnectionSettings
    {
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int TimeoutConnectMs { get; set; }
    }
}
