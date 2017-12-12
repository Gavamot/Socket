using Newtonsoft.Json;
using Service.Core;
using Service.DataClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Configuration;

namespace Service.Services
{
    public class DeviceService
    {
        private DevicesListItem[] DevicesCash { get; set; }

        private ILogger log { get; set; }
        private Thread devicesCashThread { get; set; }
        public DateTime devicesCashLastUpdate { get; private set; } = DateTime.MinValue;
        public ServiceConfig config { get; set; }

        public DeviceService(ServiceConfig config, ILogger logger)
        {
            this.config = config;
            this.log = logger;
        }

#region devicesCashUpdate

       public void Start()
       {
            devicesCashThread = CreateUpdateDevicesCashThread();
            devicesCashThread.Start();
       }

        private Thread CreateUpdateDevicesCashThread()
        {
            Thread res = new Thread(UpdateDevicesCashRun);
            res.Name = "Devices cash update";
            res.Priority = ThreadPriority.Highest;
            return res;
        }

        private void UpdateDevicesCashRun()
        {
            while (true)
            {
                try
                {
                    UpdateDevicesCash();
                    Thread.Sleep(config.DelayBetweenReqvestMs);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void UpdateDevicesCash()
        {
            try
            {
                var message = Message.CreateStandartMessage(EASCMessage.eWebGetBrigadesInfo);
                DevicesCash = AscPool.Instance.Get<DevicesListItem[]>(message, config);
                devicesCashLastUpdate = DateTime.Now;
                Console.WriteLine($"{devicesCashLastUpdate:yyyy-MM-dd HH:mm:ss} - {Thread.CurrentThread.Name} - данные успешно обновленны");
            }
            catch(Exception e)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} - Не удалось обновить данные");
            }
        }
#endregion

        public ResponseMessage<DevicesListItem[]> GetDevicesList()
        {
            return new ResponseMessage<DevicesListItem[]>(devicesCashLastUpdate, DevicesCash);
        }

        public ResponseMessage<DevicesListItem> GetByBrigadeCode(string code)
        {
            var device = DevicesCash.First(x => x.Position.Brigade == code);
            return new ResponseMessage<DevicesListItem>(devicesCashLastUpdate, device);
        }

        public ResponseMessage<DevicesListItem[]> GetByBrigadeCodeList(string[] codes)
        {
            var devices = DevicesCash.Where(x => codes.Contains(x.Position.Brigade)).ToArray();
            return new ResponseMessage<DevicesListItem[]>(devicesCashLastUpdate, devices);
        }
    }
}
