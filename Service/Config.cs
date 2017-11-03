using System.Net;

namespace Service
{
    public class AscConfig
    {
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int DelayMsConnect { get; set; }
        public int DelayMsRequest { get; set; }
        public int DelayMsSend { get; set; }
        public int Tries { get; set; }

        public static AscConfig GetFakeConfig()
        {
            AscConfig res = new AscConfig
            {
                IpAdress = "192.168.1.46",
                Port = 1336,
                DelayMsConnect = 15000,
                DelayMsRequest = 1000 * 60 * 60,
                DelayMsSend = 10000 * 60 *60,
                Tries = 10
            };
            return res;
        }
    }

    public class Config
    {
        public AscConfig AscConfig { get; set; }
    }
}