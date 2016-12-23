namespace MemoryBus
{
    using System;

    internal abstract class ResponderBase<T> : LocalDisposable
    {
        protected Func<T, bool> _filter;
        public ResponderBase(Func<T, bool> filter)
        {
            _filter = filter;
        }

        public bool CanRespond(T message) => _filter == null ? true : _filter(message);
    }
}
