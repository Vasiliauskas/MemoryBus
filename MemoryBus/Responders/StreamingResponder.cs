namespace MemoryBus.Responders
{
    using System;
    class StreamingResponder<T, U> : ResponderBase<T>
    {
        private Action<T, IObserver<U>> _responder;
        internal StreamingResponder(Action<T, IObserver<U>> responder, Func<T, bool> filter)
            : base(filter)
        {
            if (responder == null)
                throw new ArgumentNullException("Responder cannot be null");

            _responder = responder;
        }

        public void Respond(T message, IObserver<U> observer) => _responder(message, observer);
    }
}
