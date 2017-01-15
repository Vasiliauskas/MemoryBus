namespace MemoryBus
{
    using System;
    using System.Threading.Tasks;

    public interface IBus : IDisposable
    {
        void Publish<TRequest>(TRequest request);
        Task PublishAsync<TRequest>(TRequest request);

        IDisposable Subscribe<TRequest>(Action<TRequest> handler);
        IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter);
        IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler);
        IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter);

        UResponse Request<TRequest, UResponse>(TRequest request);
        Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request);

        IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler);
        IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter);
        IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler);
        IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter);

        IObservable<UResponse> StreamRequest<TRequest, UResponse>(TRequest request);
        IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError, Action onCompleted);
        IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action<Exception> onError);
        IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext, Action onCompleted);
        IDisposable StreamRequest<TRequest, UResponse>(TRequest request, Action<UResponse> onNext);

        IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler, Func<TRequest, bool> filter);
        IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler);
    }
}
