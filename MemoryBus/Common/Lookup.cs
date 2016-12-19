using System.Collections.Generic;
using System.Threading;

namespace MemoryBus.Common
{
    internal class Lookup<T, U> where U : class
    {
        protected readonly ReaderWriterLock _lock;
        protected readonly Dictionary<T, U> _dictionary;
        protected readonly int _readerTimeout;
        protected readonly int _writerTimeout;

        internal Lookup(int readerTimeout, int writerTimeout)
        {
            _lock = new ReaderWriterLock();
            _dictionary = new Dictionary<T, U>();
            _readerTimeout = readerTimeout;
            _writerTimeout = writerTimeout;
        }

        internal U GetMember(T key)
        {
            U member = null;
            _lock.AcquireReaderLock(_readerTimeout);
            if (_dictionary.ContainsKey(key))
                member = _dictionary[key];
            _lock.ReleaseReaderLock();

            return member;
        }

        internal void AddMember(T key, U member)
        {
            _lock.AcquireWriterLock(_writerTimeout);
            _dictionary[key] = member;
            _lock.ReleaseWriterLock();
        }
    }
}
