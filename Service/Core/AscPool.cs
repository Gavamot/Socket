using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Services;

namespace Service.Core
{
    public class AscPool
    {
        private MyObjectPool<AscClient> pool;
        protected ILogger log;
        protected AscConfig config;

        public AscPool(AscConfig config, ILogger logger)
        {
            Init(config, logger);
        }

        protected void Init(AscConfig config, ILogger logger)
        {
            if(pool != null)
                throw new Exception("Пул уже был создан");
            this.log = logger;
            this.config = config;
            CreatePool(this.config, logger);
        }

        private void CreatePool(AscConfig config, ILogger log)
        {
            var ascPool = new AscClient[config.SocketCount];
            for (int i = 0; i < config.SocketCount; i++)
            {
                ascPool[i] = new AscClient($"ascClient_{i}", config, log);
                bool isStarted = ascPool[i].StartClient();
            }
            pool = new MyObjectPool<AscClient>(ascPool);
        }

        private bool IsResEmpty<T>(T res)
        {
            return res == null || res.Equals(default(T));
        }

        public T Get<T>(Message msg)
        {
            int tryes = config.Tries;
            var res = default(T);
            while (IsResEmpty(res) && tryes > 0)
            {
                var client = pool.GetObject();
                try
                {
                    res = client.Reqvest<T>(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    string theadName = Thread.CurrentThread.Name;
                    try
                    {
                        client?.StopClient();
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"В потока { theadName } при выключении сокета произошла ошибка socet {client?.Name}", ex);
                    }
                    client = new AscClient(client?.Name, config, log);
                }
                pool.PutObject(client);
                if(IsResEmpty(res))
                    Thread.Sleep(config.DelayMsTries);
                tryes--;
            }
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