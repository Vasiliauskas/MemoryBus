namespace MemoryBus
{
    using System;
    using System.Threading.Tasks;

    internal class AsyncResponder<T, U> : IDisposable 
    {
        private Func<T, Task<U>> _responder;
        internal AsyncResponder(Func<T, Task<U>> responder)
        {
            if (responder == null)
                throw new ArgumentNullException("Responder cannot be null");

            _responder = responder;
        }

        public void Dispose() => _responder = null;
    }
}
