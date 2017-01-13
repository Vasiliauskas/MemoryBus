namespace MemoryBus
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading;
    using Bus;

    public sealed class MemoryBus : IBus
    {
        private PublishSubscribeBus _pubSubBus;
        private RequestRespondBus _reqRespBus;
        private RequestRespondStreamingBus _streamingReqRespBus;

        private bool _isDisposed;
        private IBusConfig _config;

        public MemoryBus(IBusConfig config)
        {
            _config = config;
            _pubSubBus = new PublishSubscribeBus();
            _reqRespBus = new RequestRespondBus();
            _streamingReqRespBus = new RequestRespondStreamingBus();
        }

        public void Publish<TRequest>(TRequest message) => _pubSubBus.Publish(message);

        public async Task PublishAsync<TRequest>(TRequest message) => await _pubSubBus.PublishAsync(message);


        public IDisposable Subscribe<TRequest>(Action<TRequest> handler) => Subscribe<TRequest>(handler, null);

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter) => _pubSubBus.Subscribe(handler, filter);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler) => SubscribeAsync<TRequest>(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter) => _pubSubBus.SubscribeAsync<TRequest>(handler, filter);


        public UResponse Request<TRequest, UResponse>(TRequest request) => _reqRespBus.Request<TRequest, UResponse>(request);

        public async Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request) => await _reqRespBus.RequestAsync<TRequest, UResponse>(request);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler) => Respond(handler, null);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter) => _reqRespBus.Respond(handler, filter);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler) => RespondAsync(handler, null);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter) => _reqRespBus.RespondAsync(handler, filter);

        public IObservable<UResponse> StreamRequest<TRequest, UResponse>(TRequest request)
        {
            throw new NotImplementedException();
        }

        public void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action<Exception> onError, Action onCompleted)
        {
            throw new NotImplementedException();
        }

        public void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action<Exception> onError)
        {
            throw new NotImplementedException();
        }

        public void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action onCompleted)
        {
            throw new NotImplementedException();
        }

        public void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext)
        {
            throw new NotImplementedException();
        }

        public IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler, Func<TRequest, bool> filter)
        {
            throw new NotImplementedException();
        }

        public IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _pubSubBus.Dispose();
            _reqRespBus.Dispose();
            _streamingReqRespBus.Dispose();

            _isDisposed = true;
        }
    }
}
