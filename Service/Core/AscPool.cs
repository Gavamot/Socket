using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Configuration;
using Service.Services;

namespace Service.Core
{
    public class AscPool
    {
        private  MyObjectPool<AscClient> pool;
        protected ILogger log;
        protected ConnectionSettings config;

        public static AscPool Instance { get; private set; }

        public AscPool(ConnectionSettings config, int connectionPoolSize, ILogger logger)
        {
            this.log = logger;
            this.config = config;
            CreatePool(config, connectionPoolSize, logger);
        }

        public static void Init(ConnectionSettings config, int connectionPoolSize, ILogger log)
        {
            if(Instance != null)
                throw new Exception("Пул уже был создан");
            Instance = new AscPool(config, connectionPoolSize, log);
        }

        private void CreatePool(ConnectionSettings connectionSettings, int connectionPoolSize, ILogger log)
        {
            var ascPool = new AscClient[connectionPoolSize];
            for (int i = 0; i < connectionPoolSize; i++)
            {
                ascPool[i] = new AscClient(connectionSettings, log);
                ascPool[i].StartClient();
            }
            pool = new MyObjectPool<AscClient>(ascPool);
        }

        private bool IsResEmpty<T>(T res)
        {
            return res == null || res.Equals(default(T));
        }

        public T Get<T>(Message msg, ServiceConfig serviceSettings)
        {
            var res = default(T);
            var client = pool.GetObject();
            try
            {
                res = client.Reqvest<T>(msg, serviceSettings);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                string theadName = Thread.CurrentThread.Name;
                try
                {
                    client.StopClient();
                }
                catch (Exception ex)
                {
                    log.LogError($"В потока { theadName } при выключении сокета произошла ошибка socet", ex);
                }
                client = new AscClient(config, log);
            }
            pool.PutObject(client);
            if (res == null)
                throw new Exception("Не получилось получить данные");
            return res;
        } 

        class MyObjectPool<T>
        {
            readonly ConcurrentBag<T> _objects;
           
            public MyObjectPool(IEnumerable<T> set)
            {
                _objects = new ConcurrentBag<T>();
                foreach (var item in set)
                    _objects.Add(item);
            }

            public T GetObject()
            {
                T item;
                while (!_objects.TryTake(out item))
                    Thread.Sleep(1);
                return item;
            }

            public void PutObject(T item)
            {
                _objects.Add(item);
            }
        }
    }
}