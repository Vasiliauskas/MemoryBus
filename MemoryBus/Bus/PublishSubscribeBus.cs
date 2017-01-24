namespace MemoryBus.Bus
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class PublishSubscribeBus : BusBase
    {
        public void Publish<TMessage>(TMessage message)
        {
            var subs = GetSubscribers<TMessage>();
            if (subs != null)
            {
                foreach (var syncSub in subs.Item1)
                    syncSub.Consume(message);

                foreach (var asyncSub in subs.Item2)
                    asyncSub.ConsumeAsync(message).Wait();
            }
        }

        public async Task PublishAsync<TMessage>(TMessage request)
        {
            var subs = GetSubscribers<TMessage>();
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

        public IDisposable Subscribe<TMessage>(Action<TMessage> handler) => Subscribe<TMessage>(handler, null);

        public IDisposable Subscribe<TMessage>(Action<TMessage> handler, Func<TMessage, bool> filter)
        {
            var topic = Topic.CreateTopic<TMessage>();
            var subscriber = new Subscriber<TMessage>(handler, filter);

            return Subscribe(topic, subscriber);
        }

        public IDisposable SubscribeAsync<TMessage>(Func<TMessage, Task> handler) => SubscribeAsync(handler, null);

        public IDisposable SubscribeAsync<TMessage>(Func<TMessage, Task> handler, Func<TMessage, bool> filter)
        {
            var topic = Topic.CreateTopic<TMessage>();
            var subscriber = new AsyncSubscriber<TMessage>(handler, filter);

            return Subscribe(topic, subscriber);
        }

        private Tuple<IEnumerable<Subscriber<TMessage>>, IEnumerable<AsyncSubscriber<TMessage>>> GetSubscribers<TMessage>()
        {
            List<IDisposable> subscribers;
            var topic = Topic.CreateTopic<TMessage>();

            if (_handlers.TryGet(topic, out subscribers))
            {
                var asyncSubscribers = subscribers.Where(s => s is AsyncSubscriber<TMessage>).Cast<AsyncSubscriber<TMessage>>();
                var syncSubscribers = subscribers.Where(s => s is Subscriber<TMessage>).Cast<Subscriber<TMessage>>();

                return new Tuple<IEnumerable<Subscriber<TMessage>>, IEnumerable<AsyncSubscriber<TMessage>>>(syncSubscribers, asyncSubscribers);
            }

            return null;
        }


    }
}
