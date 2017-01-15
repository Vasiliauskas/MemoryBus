namespace MemoryBus
{
    using System;
    using System.Threading.Tasks;

    class AsyncResponder<T, U> : ResponderBase<T>
    {
        private Func<T, Task<U>> _responder;
        internal AsyncResponder(Func<T, Task<U>> responder, Func<T, bool> filter)
            : base(filter)
        {
            if (responder == null)
                throw new ArgumentNullException("Responder cannot be null");

            _responder = responder;
        }

        public Task<U> RespondAsync(T message) => _responder(message);
    }
}
