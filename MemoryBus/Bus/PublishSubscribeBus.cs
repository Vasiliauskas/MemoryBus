namespace MemoryBus.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class PublishSubscribeBus : BusBase
    {
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

            return Subscribe(key, subscriber);
        }

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler) => SubscribeAsync(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
        {
            var key = typeof(TRequest).GetHashCode();
            var subscriber = new AsyncSubscriber<TRequest>(handler, filter);

            return Subscribe(key, subscriber);
        }

        private Tuple<IEnumerable<Subscriber<T>>, IEnumerable<AsyncSubscriber<T>>> GetSubscribers<T>()
        {
            List<IDisposable> subscribers;
            var key = typeof(T).GetHashCode();
            if (_handlers.TryGet(key, out subscribers))
            {
                var asyncSubscribers = subscribers.Where(s => s is AsyncSubscriber<T>).Cast<AsyncSubscriber<T>>();
                var syncSubscribers = subscribers.Where(s => s is Subscriber<T>).Cast<Subscriber<T>>();

                return new Tuple<IEnumerable<Subscriber<T>>, IEnumerable<AsyncSubscriber<T>>>(syncSubscribers, asyncSubscribers);
            }

            return null;
        }

       
    }
}
