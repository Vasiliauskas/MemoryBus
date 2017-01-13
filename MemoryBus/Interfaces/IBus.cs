﻿namespace MemoryBus
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
        void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action<Exception> onError, Action onCompleted);
        void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action<Exception> onError);
        void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext, Action onCompleted);
        void StreamRequest<TRequest, UResponse>(TRequest request, Action<TRequest> onNext);

        IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler, Func<TRequest, bool> filter);
        IDisposable StreamRespond<TRequest, UResponse>(Action<TRequest, IObserver<UResponse>> handler);
    }
}
