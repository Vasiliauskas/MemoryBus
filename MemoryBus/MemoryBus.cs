namespace MemoryBus
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Collections.Concurrent;

    public sealed class MemoryBus : IBus, IDisposable
    {
        private ConcurrentDictionary<string, List<IDisposable>> _subscribers;
        private ConcurrentDictionary<string, List<IDisposable>> _asyncSubscribers;
        private ConcurrentDictionary<string, List<IDisposable>> _responders;
        private ConcurrentDictionary<string, List<IDisposable>> _asyncResponders;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _subscribers = new ConcurrentDictionary<string, List<IDisposable>>();
            _asyncSubscribers = new ConcurrentDictionary<string, List<IDisposable>>();
            _responders = new ConcurrentDictionary<string, List<IDisposable>>();
            _asyncResponders = new ConcurrentDictionary<string, List<IDisposable>>();
        }

        public void Publish<TRequest>(TRequest message)
        {
            List<IDisposable> consumers;
            if (_subscribers.TryGetValue(typeof(TRequest).FullName, out consumers))
                consumers.ForEach(c => (c as Subscriber<TRequest>).Consume(message));
        }

        public async Task PublishAsync<TRequest>(TRequest request)
        {
            List<IDisposable> subscribers;
            var key = typeof(TRequest).FullName;
            if (_asyncSubscribers.TryGetValue(key, out subscribers))
            {
                await Task
                .WhenAll(subscribers.Select(s => (s as AsyncSubscriber<TRequest>).Consume(request)))
                .ContinueWith(r =>
                {
                    if (r.Exception != null)
                        throw r.Exception;
                });
            }
            else
            {
                throw new InvalidOperationException($"Failed to publish async {key}");
            }
        }

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler) => Subscribe<TRequest>(handler, null);

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).FullName;
            var subscriber = new Subscriber<TRequest>(handler, filter);

            return Subscribe(key, subscriber, _subscribers);
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler) => SubscribeAsync(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).FullName;
            var subscriber = new AsyncSubscriber<TRequest>(handler);

            return Subscribe(key, subscriber, _asyncSubscribers);
        }

        public UResponse Request<TRequest, UResponse>(TRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request)
        {
            throw new NotImplementedException();
        }

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler)
        {
            var key = typeof(TRequest).FullName + typeof(UResponse).FullName;
            var responder = new Responder<TRequest, UResponse>(handler);

            return Subscribe(key, responder, _responders);
        }

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).FullName + typeof(UResponse).FullName;
            var responder = new Responder<TRequest, UResponse>(handler);

            return Subscribe(key, responder, _responders);
        }

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler)
        {
            var key = typeof(TRequest).FullName + typeof(UResponse).FullName;
            var responder = new AsyncResponder<TRequest, UResponse>(handler);

            return Subscribe(key, responder, _asyncResponders);
        }

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).FullName + typeof(UResponse).FullName;
            var responder = new AsyncResponder<TRequest, UResponse>(handler);

            return Subscribe(key, responder, _asyncResponders);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _subscribers?.Clear();
            _asyncSubscribers?.Clear();
            _responders?.Clear();
            _asyncResponders?.Clear();

            _isDisposed = true;
        }

        private IDisposable Subscribe(string key, IDisposable subscriber, ConcurrentDictionary<string, List<IDisposable>> collection)
        {
            if (!collection.ContainsKey(key))
                if (!collection.TryAdd(key, new List<IDisposable>()))
                    throw new InvalidOperationException($"Failed to subscribe {key}");

            collection[key].Add(subscriber);

            return new DisposableHandle(() => Unsubscribe(key, subscriber, collection));
        }

        private void Unsubscribe(string key, IDisposable subscriber, ConcurrentDictionary<string, List<IDisposable>> collection)
        {
            List<IDisposable> outValue;
            if (collection.TryRemove(key, out outValue))
                subscriber.Dispose();
            else
                throw new InvalidOperationException($"Failed to unsubscribe {key}");
        }
    }
}
