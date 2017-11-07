using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Service.Core
{
    public class AscPool
    {
        private MyObjectPool<AscClient> pool;
        public readonly int Size;
        private readonly ILogger log;
        private readonly AscConfig config;

        public AscPool(int size, AscConfig config, ILogger logger)
        {
            this.Size = size;
            this.log = logger;
            this.config = config;
            CreatePool(size, config, logger);
        }

        private void CreatePool(int size, AscConfig config, ILogger log)
        {
            var ascPool = new AscClient[size];
            for (int i = 0; i < size; i++)
            {
                ascPool[i] = new AscClient($"ascClient_{i}", config, log);
                bool isStarted = ascPool[i].StartClient();
            }
            pool = new MyObjectPool<AscClient>(ascPool);
        }
        
        public T Get<T>(Message msg)
        {
            var res = default(T);
            var client = pool.GetObject();
            try
            {
                res = client.Reqvest<T>(msg);
                Console.WriteLine($"T={Thread.CurrentThread.Name}  C={client.Name}");
            }
            catch (Exception e)
            {
                string theadName = Thread.CurrentThread.Name;
                log.LogError($"В потоке { theadName } произошла ошибка. socet {client?.Name}", e);
                try
                {
                    client?.StopClient();
                    log.LogError($"В потоке { theadName } перезапущен сокет {client?.Name}");
                }
                catch (Exception ex)
                {
                    log.LogError($"В потока { theadName } при выключении сокета произошла ошибка socet {client?.Name}", ex);
                }
                client = new AscClient(client?.Name, config, log);
            }
            pool.PutObject(client);
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
