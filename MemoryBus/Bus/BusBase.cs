namespace MemoryBus.Bus
{
    using Common;
    using System;

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

        protected IDisposable Subscribe(Topic topic, IDisposable subscriber)
        {
            if (!_handlers.TryAdd(topic, subscriber))
                throw new InvalidOperationException($"Failed to subscribe {topic}");

            return new DisposableHandle(() => Unsubscribe(topic, subscriber));
        }

        protected void Unsubscribe(Topic topic, IDisposable subscriber)
        {
            if (_handlers.TryRemove(topic, subscriber))
                subscriber.Dispose();
            else
                throw new InvalidOperationException($"Failed to remove member '{topic}'");
        }
    }
}
