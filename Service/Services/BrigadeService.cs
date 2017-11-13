using Newtonsoft.Json;
using Service.Core;
using Service.DataClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Services
{
    public class BrigadeService
    {
        private DevicesListItem[] DevicesCash { get; set; }
        private AscPool pool { get; set; } 

        private int devicesCashUpdateTime { get; set; }
        private Thread devicesCashThread { get; set; }
        public DateTime devicesCashLastUpdate { get; private set; } = DateTime.MinValue;

        //private static BrigadeService self;
        public BrigadeService(AscPool pool, int devicesCashUpdateTime)
        {
            this.pool = pool;
            this.devicesCashUpdateTime = devicesCashUpdateTime;
        }

        //public static BrigadeService GetInstance(AscPool pool, int devicesCashUpdateTime)
        //{
        //    if (self == null)
        //        self = new BrigadeService(pool, devicesCashUpdateTime);
        //    return self;
        //}

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
                    Thread.Sleep(devicesCashUpdateTime);
                }catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private void UpdateDevicesCash()
        {
            var message = Message.CreateStandartMessage(EASCMessage.eWebGetBrigadesInfo);
            var res = pool.Get<DevicesListItem[]>(message);
            if (res == default(DevicesListItem[]))
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} - Не удалось обновить данные");
                //Debug.Print($"{Thread.CurrentThread.Name} - Не удалось обновить данные");
                return;
            }
            devicesCashLastUpdate = DateTime.Now;
            DevicesCash = res;
            Console.WriteLine($"{Thread.CurrentThread.Name} - данные успешно обновленны");
            //Debug.Print($"{Thread.CurrentThread.Name} - данные успешно обновленны");
        }
#endregion

        public ResponseMessage<DevicesListItem[]> GetDevicesList()
        {
            var res = new ResponseMessage<DevicesListItem[]>(devicesCashLastUpdate, DevicesCash);
            return res;
        }
    }
}
