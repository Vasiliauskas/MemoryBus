namespace MemoryBus.Bus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class RequestRespondBusBase : BusBase
    {
        protected ResponderBase<TRequest> GetResponder<TRequest, UResponse>(TRequest request)
        {
            List<IDisposable> responders;
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));

            if (_handlers.TryGet(key, out responders))
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

        protected int GetCombinedHashCode(Type request, Type response)
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
