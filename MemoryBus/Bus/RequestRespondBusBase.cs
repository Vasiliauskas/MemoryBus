namespace MemoryBus.Bus
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class RequestRespondBusBase : BusBase
    {
        protected ResponderBase<TRequest> GetResponder<TRequest, UResponse>(TRequest request)
        {
            List<IDisposable> responders;
            var topic = GetCombinedTopic<TRequest, UResponse>();

            if (_handlers.TryGet(topic, out responders))
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

        protected Topic GetCombinedTopic<TRequest, UResponse>() 
            => Topic.CreateTopic<TRequest,UResponse>();
    }
}
