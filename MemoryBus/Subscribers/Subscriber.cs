namespace MemoryBus
{
    using System;

    internal class Subscriber<T> : SubscriberBase<T>, IDisposable
    {
        private Action<T> _handler;
        internal Subscriber(Action<T> handler, Func<T, bool> filter = null)
            : base(filter)
        {
            if (handler == null)
                throw new ArgumentNullException("Handler cannot be null");
            _handler = handler;
        }

        public void Consume(T message)
        {
            if (CanConsume(message))
                _handler(message);
        }

        public void Dispose()
        {
            _handler = null;
            _filter = null;
        }
    }
}
