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
        private ConcurrentDictionary<Type, List<IDisposable>> _subscribers;
        private ConcurrentDictionary<Type, List<IDisposable>> _asyncSubscribers;
        private ConcurrentDictionary<Type, List<IDisposable>> _responders;
        private ConcurrentDictionary<Type, List<IDisposable>> _asyncResponders;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _subscribers = new ConcurrentDictionary<Type, List<IDisposable>>();
            _asyncSubscribers = new ConcurrentDictionary<Type, List<IDisposable>>();
        }

        public void Publish<TRequest>(TRequest message)
        {
            List<IDisposable> consumers;
            if (_subscribers.TryGetValue(typeof(TRequest), out consumers))
                consumers.ForEach(c => (c as Subscriber<TRequest>).Consume(message));
        }

        public async Task PublishAsync<TRequest>(TRequest request)
        {
            List<IDisposable> subscribers;
            var key = typeof(TRequest);
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
                throw new InvalidOperationException($"Failed to publish async {key.FullName}");
            }
        }

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler) => Subscribe<TRequest>(handler, null);

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest);
            var subscriber = new Subscriber<TRequest>(handler, filter);

            return Subscribe(key, subscriber, _subscribers);
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler) => SubscribeAsync(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest);
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

        public void Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler)
        {
            throw new NotImplementedException();
        }

        public void Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler)
        {
            throw new NotImplementedException();
        }

        public Task RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter)
        {
            throw new NotImplementedException();
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

        private IDisposable Subscribe(Type key, IDisposable subscriber, ConcurrentDictionary<Type,List<IDisposable>> collection)
        {
            if (!collection.ContainsKey(key))
                if (!collection.TryAdd(key, new List<IDisposable>()))
                    throw new InvalidOperationException($"Failed to subscribe {key.FullName}");

            collection[key].Add(subscriber);

            return new DisposableHandle(() => Unsubscribe(key, subscriber, collection));
        }

        private void Unsubscribe(Type key, IDisposable subscriber, ConcurrentDictionary<Type, List<IDisposable>> collection)
        {
            List<IDisposable> outValue;
            if (collection.TryRemove(key, out outValue))
                subscriber.Dispose();
            else
                throw new InvalidOperationException($"Failed to unsubscribe {key.FullName}");
        }
    }
}
