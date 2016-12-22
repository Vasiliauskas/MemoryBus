namespace MemoryBus
{
    using System;
    using System.Threading.Tasks;

    internal class AsyncSubscriber<T> : SubscriberBase<T>, IDisposable
    {
        private Func<T, Task> _handler;
        public AsyncSubscriber(Func<T, Task> handler, Func<T, bool> filter = null)
            : base(filter)
        {
            if (handler == null)
                throw new ArgumentNullException("Handler cannot be null");
            _handler = handler;
        }

        public async Task ConsumeAsync(T message)
        {
            if (CanConsume(message))
                await _handler(message);
        }

        public void Dispose()
        {
            _handler = null;
            _filter = null;
        }
    }
}
