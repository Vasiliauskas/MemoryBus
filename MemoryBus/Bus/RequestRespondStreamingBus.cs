namespace MemoryBus.Bus
{
    using Responders;
    using System;
    using System.Reactive.Subjects;

    class RequestRespondStreamingBus : RequestRespondBusBase
    {
        public IObservable<UResponse> Request<TRequest, UResponse>(TRequest request)
        {
            var result = TrySetupResponse<TRequest, UResponse>(request);
            result.Item2.Respond(request, result.Item1);

            return result.Item1;
        }

        public IDisposable Request<TRequest, UResponse>(TRequest request, Action<UResponse> onNext)
        {
            var result = TrySetupResponse<TRequest, UResponse>(request);
            var disposable = result.Item1.Subscribe(onNext);
            result.Item2.Respond(request, result.Item1);

            return disposable;
        }

        public IDisposable Request<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError)
        {
            var result = TrySetupResponse<TRequest, UResponse>(request);
            var disposable = result.Item1.Subscribe(onNext, onError);
            result.Item2.Respond(request, result.Item1);

            return disposable;
        }

        public IDisposable Request<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action onCompleted)
        {
            var result = TrySetupResponse<TRequest, UResponse>(request);
            var disposable = result.Item1.Subscribe(onNext, onCompleted);
            result.Item2.Respond(request, result.Item1);

            return disposable;
        }

        public IDisposable Request<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError, Action onCompleted)
        {
            var result = TrySetupResponse<TRequest, UResponse>(request);
            var disposable = result.Item1.Subscribe(onNext, onError, onCompleted);
            result.Item2.Respond(request, result.Item1);

            return disposable;
        }

        public IDisposable Respond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler) => Respond(handler, null);

        public IDisposable Respond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler, Func<TRequest, bool> filter)
        {
            var key = GetCombinedHashCode(typeof(TRequest), typeof(UResponse));
            var responder = new StreamingResponder<TRequest, UResponse>(handler, filter);

            return Subscribe(key, responder);
        }

        private Tuple<ISubject<UResponse>, StreamingResponder<TRequest, UResponse>> TrySetupResponse<TRequest, UResponse>(TRequest request)
        {
            var responder = GetResponder<TRequest, UResponse>(request) as StreamingResponder<TRequest, UResponse>;
            var subject = new ReplaySubject<UResponse>();

            return new Tuple<ISubject<UResponse>, StreamingResponder<TRequest, UResponse>>(subject, responder);
        }
    }
}
