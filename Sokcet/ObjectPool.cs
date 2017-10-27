using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sokcet
{
    /// <summary>
    /// Реализация пула объектов, использующего "мягкие" ссылки
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class
    {
        /// <summary>
        /// Объект синхронизации
        /// </summary>
        private Semaphore semaphore;

        /// <summary>
        /// Коллекция содержит управляемые объекты
        /// </summary>
        private ArrayList pool;

        /// <summary>
        /// Количество объектов, существующих в данный момент
        /// </summary>
        private Int32 instanceCount;

        /// <summary>
        /// Максимальное количество управляемых пулом объектов
        /// </summary>
        private Int32 maxInstances;

    

        /// <summary>
        /// Создание пула объектов
        /// </summary>
        /// <param name="creator">Объект, которому пул будет делегировать ответственность
        /// за создание управляемых им объектов</param>
        /// <param name="maxInstances">Максимальное количество экземпляров классов,
        /// которым пул разрешает существовать одновременно
        /// </param>
        public ObjectPool(IEnumerable<T> objects)
        {
            if(objects == null || !objects.Any())
                throw new ArgumentException("Нет обьектов для создания пула");
            this.instanceCount = 0;
            var _ = objects.ToArray();
            this.pool = new ArrayList(_);
            this.semaphore = new Semaphore(0, _.Length);
        }

        /// <summary>
        /// Возвращает количество объектов в пуле, ожидающих повторного
        /// использования. Реальное количество может быть меньше
        /// этого значения, поскольку возвращаемая 
        /// величина - это количество "мягких" ссылок в пуле.
        /// </summary>
        public Int32 Size
        {
            get
            {
                lock (pool)
                {
                    return pool.Count;
                }
            }
        }

        /// <summary>
        /// Возвращает количество управляемых пулом объектов,
        /// существующих в данный момент
        /// </summary>
        public Int32 InstanceCount { get { return instanceCount; } }


        /// <summary>
        /// Возвращает из пула объект. Если количество управляемых пулом объектов не 
        /// больше или равно значению, возвращаемому методом 
        /// объектов превышает это значение, то данный метод возварщает null 
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            lock (pool)
            {
                T thisObject = RemoveObject();
                if (thisObject != null)
                    return thisObject;

                return null;
            }
        }

        /// <summary>
        /// Возвращает из пула объект. При пустом пуле будет создан
        /// объект, если количество управляемых пулом объектов не 
        /// больше или равно значению, возвращаемому методом 
        /// <see cref="ObjectPool{T}.MaxInstances"/>. Если количество управляемых пулом 
        /// объектов превышает это значение, то данный метод будет ждать до тех
        /// пор, пока какой-нибудь объект не станет доступным для
        /// повторного использования.
        /// </summary>
        /// <returns></returns>
        public T WaitForObject()
        {
            lock (pool)
            {
                T thisObject = RemoveObject();
                if (thisObject != null)
                    return thisObject;
            }
            semaphore.WaitOne();
            return WaitForObject();
        }

        /// <summary>
        /// Удаляет объект из коллекции пула и возвращает его 
        /// </summary>
        /// <returns></returns>
        private T RemoveObject()
        {
            while (pool.Count > 0)
            {
                var refThis = (WeakReference)pool[pool.Count - 1];
                pool.RemoveAt(pool.Count - 1);
                var thisObject = (T)refThis.Target;
                if (thisObject != null)
                    return thisObject;
                instanceCount--;
            }
            return null;
        }

        /// <summary>
        /// Освобождает объект, помещая его в пул для
        /// повторного использования
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void Release(T obj)
        {
            if (obj == null)
                throw new NullReferenceException();
            lock (pool)
            {
                var refThis = new WeakReference(obj);
                pool.Add(refThis);
                semaphore.Release();
            }
        }
    }
}
