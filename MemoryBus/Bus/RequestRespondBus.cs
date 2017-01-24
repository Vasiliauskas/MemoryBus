namespace MemoryBus.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class RequestRespondBus : RequestRespondBusBase
    {
        public UResponse Request<TRequest, UResponse>(TRequest request)
        {
            var responder = GetResponder<TRequest, UResponse>(request, _handlers);

            if (responder is AsyncResponder<TRequest, UResponse>)
                return (responder as AsyncResponder<TRequest, UResponse>).RespondAsync(request).Result;
            else
                return (responder as Responder<TRequest, UResponse>).Respond(request);
        }

        public async Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request)
        {
            var responder = GetResponder<TRequest, UResponse>(request, _handlers);

            if (responder is AsyncResponder<TRequest, UResponse>)
                return await (responder as AsyncResponder<TRequest, UResponse>).RespondAsync(request);
            else
                return await Task.FromResult((responder as Responder<TRequest, UResponse>).Respond(request));
        }

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler) => Respond(handler, null);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter)
        {
            var topic = GetCombinedTopic<TRequest, UResponse>();
            var responder = new Responder<TRequest, UResponse>(handler, filter);

            return Subscribe(topic, responder);
        }

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler) => RespondAsync(handler, null);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter)
        {
            var topic = GetCombinedTopic<TRequest, UResponse>();
            var responder = new AsyncResponder<TRequest, UResponse>(handler, filter);

            return Subscribe(topic, responder);
        }

        private ResponderBase<TRequest> GetResponder<TRequest, UResponse>(TRequest request, ConcurrentKeyedCollection collection)
        {
            List<IDisposable> responders;
            var topic = GetCombinedTopic<TRequest, UResponse>();

            if (collection.TryGet(topic, out responders))
            {
                var filteredResponders = responders.Cast<ResponderBase<TRequest>>().Where(r => r.CanRespond(request));

                if (filteredResponders.Count() != 1)
                    throw new InvalidOperationException($"There should be one and only responder for <{typeof(TRequest).FullName},{typeof(TRequest).FullName}>");

                return filteredResponders.First();
            }
            else
            {
                throw new InvalidOperationException($"No responders found for <{typeof(TRequest).FullName},{typeof(UResponse).FullName}>");
            }
        }
    }
}
