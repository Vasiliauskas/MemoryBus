using System;
namespace MemoryBus
{
    internal class DisposableHandle : IDisposable
    {
        private Action _dispose;
        private bool _isDisposed;
        public DisposableHandle(Action dispose)
        {
            if (dispose == null)
                throw new ArgumentNullException("Dispose handler cannot be null");

            _dispose = dispose;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _dispose();
            }
        }
    }
}
