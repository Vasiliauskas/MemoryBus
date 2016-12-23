namespace MemoryBus
{
    using System;

    internal abstract class LocalDisposable : IDisposable
    {
        private bool _isDisposed = false;

        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                DisposeLocal();
            }
        }

        protected virtual void DisposeLocal()
        {

        }
    }
}
