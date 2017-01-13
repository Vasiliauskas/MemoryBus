using System;

namespace MemoryBus.Bus
{
    abstract class BusBase : IDisposable
    {
        protected ConcurrentKeyedCollection _handlers;

        public BusBase()
        {
            _handlers = new ConcurrentKeyedCollection(50);
        }

        public void Dispose()
        {
            _handlers?.Clear();
        }

        protected IDisposable Subscribe(int key, IDisposable subscriber)
        {
            if (!_handlers.TryAdd(key, subscriber))
                throw new InvalidOperationException($"Failed to subscribe {key}");

            return new DisposableHandle(() => Unsubscribe(key, subscriber));
        }

        protected void Unsubscribe(int key, IDisposable subscriber)
        {
            if (_handlers.TryRemove(key, subscriber))
                subscriber.Dispose();
            else
                throw new InvalidOperationException($"Failed to remove member '{key}'");
        }
    }
}
