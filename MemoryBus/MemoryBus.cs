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

        public void Publish<TRequest>(TRequest message)
            => _pubSubBus.Publish(message);

        public async Task PublishAsync<TRequest>(TRequest message)
            => await _pubSubBus.PublishAsync(message);


        public IDisposable Subscribe<TRequest>(Action<TRequest> handler)
            => Subscribe<TRequest>(handler, null);

        public IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter)
            => _pubSubBus.Subscribe(handler, filter);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler)
            => SubscribeAsync<TRequest>(handler, null);

        public IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter)
            => _pubSubBus.SubscribeAsync<TRequest>(handler, filter);


        public UResponse Request<TRequest, UResponse>(TRequest request)
            => _reqRespBus.Request<TRequest, UResponse>(request);

        public async Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request)
            => await _reqRespBus.RequestAsync<TRequest, UResponse>(request);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler)
            => Respond(handler, null);

        public IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter)
            => _reqRespBus.Respond(handler, filter);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler)
            => RespondAsync(handler, null);

        public IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter)
            => _reqRespBus.RespondAsync(handler, filter);

        public IObservable<UResponse> StreamRequest<TRequest, UResponse>(TRequest request)
            => _streamingReqRespBus.Request<TRequest, UResponse>(request);

        public IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError, Action onCompleted)
            => _streamingReqRespBus.Request<TRequest, UResponse>(request, onNext, onError, onCompleted);

        public IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError) 
            => _streamingReqRespBus.Request<TRequest, UResponse>(request, onNext, onError);

        public IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action onCompleted)
            => _streamingReqRespBus.Request<TRequest, UResponse>(request, onNext, onCompleted);

        public IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext)
            => _streamingReqRespBus.Request<TRequest, UResponse>(request, onNext);

        public IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler, Func<TRequest, bool> filter)
            => _streamingReqRespBus.Respond<TRequest, UResponse>(handler, filter);

        public IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler)
            => _streamingReqRespBus.Respond<TRequest, UResponse>(handler);

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
