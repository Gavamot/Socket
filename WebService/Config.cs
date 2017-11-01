using System.Net;

namespace WebService
{
    public class AscConfig
    {
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int DelayMsConnect { get; set; }
        public int DelayMsRequest { get; set; }
        public int DelayMsSend { get; set; }
        public int Tries { get; set; }
    }

    public class Config
    {
        public AscConfig AscConfig { get; set; }

    }
}