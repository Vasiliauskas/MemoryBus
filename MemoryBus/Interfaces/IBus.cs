namespace MemoryBus
{
    using System;
    using System.Threading.Tasks;

    public interface IBus : IDisposable
    {
        void Publish<TRequest>(TRequest request);

        /// <summary>
        /// Should be used with SubscribeAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        Task PublishAsync<TRequest>(TRequest request);

        IDisposable Subscribe<TRequest>(Action<TRequest> handler);
        IDisposable Subscribe<TRequest>(Action<TRequest> handler, Func<TRequest, bool> filter);

        /// <summary>
        /// Should be used with PublishAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler);

        /// <summary>
        /// Should be used with PublishAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        IDisposable SubscribeAsync<TRequest>(Func<TRequest, Task> handler, Func<TRequest, bool> filter);

        UResponse Request<TRequest, UResponse>(TRequest request);

        /// <summary>
        /// Should be used with RespondAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        Task<UResponse> RequestAsync<TRequest, UResponse>(TRequest request);

        IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler);
        IDisposable Respond<TRequest, UResponse>(Func<TRequest, UResponse> handler, Func<TRequest, bool> filter);

        /// <summary>
        /// Should be used with RequestAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler);

        /// <summary>
        /// Should be used with RequestAsync
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        IDisposable RespondAsync<TRequest, UResponse>(Func<TRequest, Task<UResponse>> handler, Func<TRequest, bool> filter);
    }
}
