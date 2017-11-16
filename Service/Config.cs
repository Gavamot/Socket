using System.Net;

namespace Service
{
    public class AscConfig
    {
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int SocketCount { get; set; }
        public int Tries { get; set; }

        public ServicesUpdateTime ServicesUpdateTimeMs { get; set; }

        /// <summary>
        /// Задержка между попытками
        /// </summary>
        public int DelayMsTries { get; set; }

        public static AscConfig GetFakeConfig()
        {
            AscConfig res = new AscConfig
            {
                IpAdress = "192.168.1.46",
                Port = 1336,
                SocketCount = 10,
                Tries = 10,
                DelayMsTries = 3000,
                ServicesUpdateTimeMs = new ServicesUpdateTime
                {
                    BrigedeGetDeviceList = 10000,
                    BufferSize = 1000000,
                    DelayMsBetweenReqvest = 50,
                    DelayMsConnect = 5000,
                    DelayMsRequest = 1000 * 60,
                    DelayMsSend = 10000 * 60
                }
            };
            return res;
        }
    }

    public class Config
    {
        public AscConfig AscConfig { get; set; }
    }

    public class ServicesUpdateTime
    {
        public int BrigedeGetDeviceList { get; set; }
        public int BufferSize { get; set; }
        public int DelayMsBetweenReqvest { get; set; }
        public int DelayMsConnect { get; set; }
        public int DelayMsRequest { get; set; }
        public int DelayMsSend { get; set; }
    }
}