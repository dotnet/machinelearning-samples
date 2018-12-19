using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eShopDashboard.Forecast
{
    public class ObjectPool<T>
    {
        private ConcurrentBag<T> _objects;
        private Func<T> _objectGenerator;
        private int _maxPoolSize;

        public ObjectPool(Func<T> objectGenerator, int minPoolSize = 5, int maxPoolSize = 5000)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
            _maxPoolSize = maxPoolSize;

            //Measure total time of minimum objects creation 
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Create minimum number of objects in pool
            for (int i = 0; i < minPoolSize; i++)
            {
                _objects.Add(_objectGenerator());
            }

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;     
        }

        public T GetObject()
        {
            T item;
            if (_objects.TryTake(out item))
            {
                return item;
            }
            else
            {
                if(_objects.Count <= _maxPoolSize)
                    return _objectGenerator();
                else
                    throw new InvalidOperationException("MaxPoolSize reached");
            }
        }

        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}
