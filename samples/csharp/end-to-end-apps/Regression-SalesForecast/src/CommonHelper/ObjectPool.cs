using System;
using System.Collections.Concurrent;

namespace CommonHelpers
{
    public class ObjectPool<T>
    {        
        private ConcurrentBag<Tuple<T, DateTime>> _objects;
        private Func<T> _objectGenerator;
        private int _maxPoolSize;
        private int _minPoolSize;
        private double _expirationTime;

        public int CurrentPoolSize
        {
            get { return _objects.Count; }
        }

        /// <param name="objectGenerator"></param>
        /// <param name="minPoolSize">Minimum number of objects in pool, as goal. Could be less but eventually it'll tend to that number</param>
        /// <param name="maxPoolSize">Maximum number of objects in pool</param>
        /// <param name="expirationTime">Expiration Time (mlSecs) of object since added to the pool</param>
        public ObjectPool(Func<T> objectGenerator, int minPoolSize = 5, int maxPoolSize = 1000, double expirationTime = 30000)
        {
            if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
            if (minPoolSize > maxPoolSize) throw new Exception("minPoolSize cannot be higher than maxPoolSize");
            if (minPoolSize <= 0) throw new Exception("minPoolSize cannot be equal or lower than cero");
            if (maxPoolSize <= 0) throw new Exception("maxPoolSize cannot be equal or lower than cero");
            if (expirationTime <= 0) throw new Exception("expirationTime cannot be equal or lower than cero");

            _objects = new ConcurrentBag<Tuple<T, DateTime>>();
            _objectGenerator = objectGenerator;
            _maxPoolSize = maxPoolSize;
            _minPoolSize = minPoolSize;
            _expirationTime = expirationTime;

            //Measure total time of minimum objects creation 
            var watch = System.Diagnostics.Stopwatch.StartNew();

            //Create minimum number of objects in pool
            for (int i = 0; i < minPoolSize; i++)
            {
                Tuple<T, DateTime> tuple = new Tuple<T, DateTime>(_objectGenerator(), DateTime.UtcNow);              
                _objects.Add(tuple);
            }

            //Stop measuring time
            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;     
        }

        public T GetObject()
        {
            System.DateTime now = DateTime.UtcNow;
            
            while(!_objects.IsEmpty)
            {
                Tuple<T, DateTime> tuple;
                if(_objects.TryTake(out tuple))
                {
                    if (DateTime.UtcNow - tuple.Item2 < TimeSpan.FromMilliseconds(_expirationTime))
                    {
                        //object has NOT expired, so return it
                        return tuple.Item1;
                    }
                    else
                    //If object is expired, but we have less or equal the threadshold of minPoolSize, then use it    
                    {
                        if (_objects.Count <= _minPoolSize)
                        {
                            return tuple.Item1;
                        }
                    }

                    //If it gets here, do nothing with the expired-object and try to get another non-expired object
                }
            }

            // If there are no objects available in the pool, create one and return it   
            return _objectGenerator();            
        }

        public void PutObject(T item)
        {
            //Only add objects to the pool if maxPoolSize has not been reached
            if (_objects.Count < _maxPoolSize)
            {
                //Expiration time starts when adding an object to the pool, no matter if it was originally older
                Tuple<T, DateTime> tuple = new Tuple<T, DateTime>(item, DateTime.UtcNow);
                _objects.Add(tuple);
            }               
        }
    }
}
