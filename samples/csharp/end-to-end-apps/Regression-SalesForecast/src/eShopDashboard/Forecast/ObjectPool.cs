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

        public ObjectPool(Func<T> objectGenerator, int minObjects = 5)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator;
          
            //Measure total time of minimum objects creation 
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Create minimum number of objects in pool
            for (int i = 0; i < minObjects; i++)
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
                return item;
            else
                return _objectGenerator();
        }

        public void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}
