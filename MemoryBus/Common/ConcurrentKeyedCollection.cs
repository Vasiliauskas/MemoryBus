namespace MemoryBus
{
    using Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class ConcurrentKeyedCollection
    {
        private Dictionary<Topic, HashSet<IDisposable>> _innerCollection;
        private ReaderWriterLock _lock;
        private int _writerReaderTimeout;

        internal ConcurrentKeyedCollection(int writerReaderTimeout)
        {
            _innerCollection = new Dictionary<Topic, HashSet<IDisposable>>();
            _lock = new ReaderWriterLock();
            _writerReaderTimeout = writerReaderTimeout;
        }

        internal bool TryAdd(Topic topic, IDisposable value)
        {
            _lock.AcquireWriterLock(_writerReaderTimeout);
         
            HashSet<IDisposable> hashSet;
            if (!_innerCollection.TryGetValue(topic, out hashSet))
            {
                hashSet = new HashSet<IDisposable>();
                _innerCollection.Add(topic, hashSet);
            }
            var result =  hashSet.Add(value);

            _lock.ReleaseWriterLock();

            return result;
        }

        internal bool TryGet(Topic topic, out List<IDisposable> outValue)
        {
            _lock.AcquireReaderLock(_writerReaderTimeout);
            var result = false;
            outValue = null;
            if (_innerCollection.ContainsKey(topic))
            {
                outValue = _innerCollection[topic].ToList();
                result = true;
            }

            _lock.ReleaseReaderLock();

            return result;
        }

        internal bool TryRemove(Topic topic, IDisposable member)
        {
            _lock.AcquireWriterLock(_writerReaderTimeout);

            bool result = false;
            HashSet<IDisposable> hashSet;

            if (_innerCollection.TryGetValue(topic, out hashSet))
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
