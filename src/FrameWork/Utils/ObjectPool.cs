using System;
using System.Collections.Concurrent;

namespace FrameWork
{
    public class ObjectPool<T>
    {
        private readonly Func<T> _objectCreator; 
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        public ObjectPool(Func<T> objectCreator)
        {
            _objectCreator = objectCreator;

            _queue.Enqueue(_objectCreator());
        } 

        public void Enqueue(T obj)
        {
            _queue.Enqueue(obj);
        }

        public T Dequeue()
        {
            T obj;
            return _queue.TryDequeue(out obj) ? obj : _objectCreator();
        }
    }
}
