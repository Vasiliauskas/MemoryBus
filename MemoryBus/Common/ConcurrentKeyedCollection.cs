namespace MemoryBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class ConcurrentKeyedCollection
    {
        private Dictionary<int, HashSet<IDisposable>> _innerCollection;
        private ReaderWriterLock _lock;
        private int _writerReaderTimeout;

        internal ConcurrentKeyedCollection(int writerReaderTimeout)
        {
            _innerCollection = new Dictionary<int, HashSet<IDisposable>>();
            _lock = new ReaderWriterLock();
            _writerReaderTimeout = writerReaderTimeout;
        }

        internal bool TryAdd(int key, IDisposable value)
        {
            _lock.AcquireWriterLock(_writerReaderTimeout);
         
            HashSet<IDisposable> hashSet;
            if (!_innerCollection.TryGetValue(key, out hashSet))
            {
                hashSet = new HashSet<IDisposable>();
                _innerCollection.Add(key, hashSet);
            }
            var result =  hashSet.Add(value);

            _lock.ReleaseWriterLock();

            return result;
        }

        internal bool TryGet(int key, out List<IDisposable> outValue)
        {
            _lock.AcquireReaderLock(_writerReaderTimeout);
            var result = false;
            outValue = null;
            if (_innerCollection.ContainsKey(key))
            {
                outValue = _innerCollection[key].ToList();
                result = true;
            }

            _lock.ReleaseReaderLock();

            return result;
        }

        internal bool TryRemove(int key, IDisposable member)
        {
            _lock.AcquireWriterLock(_writerReaderTimeout);

            bool result = false;
            HashSet<IDisposable> hashSet;

            if (_innerCollection.TryGetValue(key, out hashSet))
                result = hashSet.Remove(member);

            _lock.ReleaseWriterLock();

            return result;
        }

        internal void Clear()
        {
            _lock.AcquireWriterLock(_writerReaderTimeout);
            _innerCollection.Clear();
            _lock.ReleaseWriterLock();
        }
    }
}
