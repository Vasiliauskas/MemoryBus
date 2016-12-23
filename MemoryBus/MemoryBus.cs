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
        private ConcurrentKeyedCollection _asyncSubscribers;
        private ConcurrentKeyedCollection _responders;
        private ConcurrentKeyedCollection _asyncResponders;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _subscribers = new ConcurrentKeyedCollection(50);
            _asyncSubscribers = new ConcurrentKeyedCollection(50);
            _responders = new ConcurrentKeyedCollection(50);
            _asyncResponders = new ConcurrentKeyedCollection(50);
        }

        public void Publish<TRequest>(TRequest message)
        {
            List<IDisposable> subscribers;
            var key = typeof(TRequest).GetHashCode();
            if (_subscribers.TryGet(key, out subscribers))
                subscribers.ForEach(c => (c as Subscriber<TRequest>).Consume(message));
            else
                throw new InvalidOperationException($"Failed to retrieve subscribers {key}");
        }

        public async Task PublishAsync<TRequest>(TRequest request)
        {
            List<IDisposable> subscribers;
            var key = typeof(TRequest).GetHashCode();
            if (_asyncSubscribers.TryGet(key, out subscribers))
            {
                await Task
                .WhenAll(subscribers.Select(s => (s as AsyncSubscriber<TRequest>).ConsumeAsync(request)))
                .ContinueWith(r =>
                {
                    if (r.Exception != null)
                        throw r.Exception;
                });
            }
            else
            {
                throw new InvalidOperationException($"Failed to retrieve subscribers async {key}");
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

            return Subscribe(key, subscriber, _asyncSubscribers);
        }

        public UResponse Request<TRequest, UResponse>(TRequest request) =>
            GetResponder<TRequest, UResponse, Responder<TRequest, UResponse>>(request, _responders).Respond(request);

        public async Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request) =>
            await GetResponder<TRequest, UResponse, AsyncResponder<TRequest, UResponse>>(request, _asyncResponders).RespondAsync(request);

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

        private KResponder GetResponder<TRequest, UResponse, KResponder>(TRequest request, ConcurrentKeyedCollection collection)
            where KResponder : ResponderBase<TRequest>
        {
            List<IDisposable> responders;
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));

            if (collection.TryGet(key, out responders))
            {
                var filteredResponders = responders.Cast<ResponderBase<TRequest>>().Where(r => r.CanRespond(request));

                if (filteredResponders.Count() != 1)
                    throw new InvalidOperationException($"There should be one and only responder for <{typeof(TRequest).FullName},{typeof(TRequest).FullName}>");

                return (KResponder)filteredResponders.First();
            }
            else
            {
                throw new InvalidOperationException($"Failed to retrieve responders for <{typeof(TRequest).FullName},{typeof(TRequest).FullName}>");
            }
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
