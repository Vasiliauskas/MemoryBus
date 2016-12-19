namespace MemoryBus
{
    using System;

    internal class Responder<T,U> : IDisposable
        where T : class
        where U : class
    {
        private Func<T, U> _responder;
        internal Responder(Func<T,U> responder)
        {
            if (responder == null)
                throw new ArgumentNullException("Responder cannot be null");

            _responder = responder;
        }

        internal U Respond(T message) => _responder(message);

        public void Dispose() => _responder = null;
    }
}
