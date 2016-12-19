namespace MemoryBus
{
    using System;

    internal abstract class SubscriberBase<T>
    {
        protected Func<T, bool> _filter;
        public SubscriberBase(Func<T,bool> filter)
        {
            _filter = filter;
        }

        public bool CanConsume(T message) => _filter == null ? true : _filter(message);
    }
}
