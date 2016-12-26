namespace MemoryBus
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;

    public sealed class MemoryBus : IBus
    {
        private ConcurrentKeyedCollection _subscribers;
        private ConcurrentKeyedCollection _responders;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _subscribers = new ConcurrentKeyedCollection(50);
            _responders = new ConcurrentKeyedCollection(50);
        }

        public void Publish<TRequest>(TRequest message)
        {
            var subs = GetSubscribers<TRequest>();
            if (subs != null)
            {
                foreach (var syncSub in subs.Item1)
                    syncSub.Consume(message);

                foreach (var asyncSub in subs.Item2)
                    asyncSub.ConsumeAsync(message).Wait();
            }
        }

        public async Task PublishAsync<TRequest>(TRequest request)
        {
            var subs = GetSubscribers<TRequest>();
            if (subs != null)
            {
                await Task
                .WhenAll(subs.Item1.Select(s => Task.Run(() => s.Consume(request))))
                .ContinueWith(r =>
                {
                    if (r.Exception != null)
                        throw r.Exception;
                });

                await Task
                .WhenAll(subs.Item2.Select(s => s.ConsumeAsync(request)))
                .ContinueWith(r =>
                {
                    if (r.Exception != null)
                        throw r.Exception;
                });
            }
        }

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler) => Subscribe<TRequest>(handler, null);

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).GetHashCode();
            var subscriber = new Subscriber<TRequest>(handler, filter);

            return Subscribe(key, subscriber, _subscribers);
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler) => SubscribeAsync(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).GetHashCode();
            var subscriber = new AsyncSubscriber<TRequest>(handler, filter);

            return Subscribe(key, subscriber, _subscribers);
        }

        public UResponse Request<TRequest, UResponse>(TRequest request)
        {
            var responder = GetResponder<TRequest, UResponse>(request, _responders);

            if (responder is AsyncResponder<TRequest, UResponse>)
                return (responder as AsyncResponder<TRequest, UResponse>).RespondAsync(request).Result;
            else
                return (responder as Responder<TRequest, UResponse>).Respond(request);
        }

        public async Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request)
        {
            var responder = GetResponder<TRequest, UResponse>(request, _responders);

            if (responder is AsyncResponder<TRequest, UResponse>)
                return await (responder as AsyncResponder<TRequest, UResponse>).RespondAsync(request);
            else
                return await Task<UResponse>.Run(() => (responder as Responder<TRequest, UResponse>).Respond(request));
        }

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler) => Respond(handler, null);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter)
        {
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));
            var responder = new Responder<TRequest, UResponse>(handler, filter);

            return Subscribe(key, responder, _responders);
        }

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler) => RespondAsync(handler, null);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter)
        {
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));
            var responder = new AsyncResponder<TRequest, UResponse>(handler, filter);

            return Subscribe(key, responder, _responders);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _subscribers?.Clear();
            _responders?.Clear();

            _isDisposed = true;
        }

        private IDisposable Subscribe(int key, IDisposable subscriber, ConcurrentKeyedCollection collection)
        {
            if (!collection.TryAdd(key, subscriber))
                throw new InvalidOperationException($"Failed to subscribe {key}");

            return new DisposableHandle(() => Unsubscribe(key, subscriber, collection));
        }

        private void Unsubscribe(int key, IDisposable subscriber, ConcurrentKeyedCollection collection)
        {
            if (collection.TryRemove(key, subscriber))
                subscriber.Dispose();
            else
                throw new InvalidOperationException($"Failed to remove member '{key}'");
        }

        private ResponderBase<TRequest> GetResponder<TRequest, UResponse>(TRequest request, ConcurrentKeyedCollection collection)
        {
            List<IDisposable> responders;
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));

            if (collection.TryGet(key, out responders))
            {
                var filteredResponders = responders.Cast<ResponderBase<TRequest>>().Where(r => r.CanRespond(request));

                if (filteredResponders.Count() != 1)
                    throw new InvalidOperationException($"There should be one and only responder for <{typeof(TRequest).FullName},{typeof(TRequest).FullName}>");

                return filteredResponders.First();
            }
            else
            {
                throw new InvalidOperationException($"No responders found for <{typeof(TRequest).FullName},{typeof(TRequest).FullName}>");
            }
        }

        private Tuple<IEnumerable<Subscriber<T>>, IEnumerable<AsyncSubscriber<T>>> GetSubscribers<T>()
        {
            List<IDisposable> subscribers;
            var key = typeof(T).GetHashCode();
            if (_subscribers.TryGet(key, out subscribers))
            {
                var asyncSubscribers = subscribers.Where(s => s is AsyncSubscriber<T>).Cast<AsyncSubscriber<T>>();
                var syncSubscribers = subscribers.Where(s => s is Subscriber<T>).Cast<Subscriber<T>>();

                return new Tuple<IEnumerable<Subscriber<T>>, IEnumerable<AsyncSubscriber<T>>>(syncSubscribers, asyncSubscribers);
            }

            return null;
        }

        private int GetCombinedHashCode(Type request, Type response)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + request.GetHashCode();
                hash = hash * 31 + response.GetHashCode();
                return hash;
            }
        }
    }
}
