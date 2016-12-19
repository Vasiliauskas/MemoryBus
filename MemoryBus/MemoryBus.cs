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
        private ConcurrentDictionary<Type, List<object>> _consumers;
        private ConcurrentDictionary<Type, List<object>> _asyncConsumers;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _consumers = new ConcurrentDictionary<Type, List<object>>();
            _asyncConsumers = new ConcurrentDictionary<Type, List<object>>();
        }

        public void Publish<TRequest>(TRequest message)
        {
            List<object> consumers;
            if (_consumers.TryGetValue(typeof(TRequest), out consumers))
                consumers.ForEach(c => (c as Subscriber<TRequest>).Consume(message));
        }

        public async Task PublishAsync<TRequest>(TRequest request)
        {
            List<object> consumers;
            var key = typeof(TRequest);
            if (_consumers.TryGetValue(key, out consumers))
            {
                await Task
                .WhenAll(consumers.Select(s => (s as AsyncSubscriber<TRequest>).Consume(request)))
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

            if (!_consumers.ContainsKey(key))
                if (!_consumers.TryAdd(key, new List<object>()))
                    throw new InvalidOperationException($"Failed to subscribe {key.FullName}");

            var subscriber = new Subscriber<TRequest>(handler, filter);
            _consumers[key].Add(subscriber);

            return new DisposableHandle(() => RemoveSubscriber(key, subscriber));
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler)
        {
            throw new NotImplementedException();
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
        {
            throw new NotImplementedException();
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
            _consumers?.Clear();
            _isDisposed = true;
        }

        private void RemoveSubscriber<TRequest>(Type key, Subscriber<TRequest> subscriber)
        {
            throw new NotImplementedException();
        }
    }
}
