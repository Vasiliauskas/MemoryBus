using System;

namespace MemoryBus
{
    public class DefaultConfig : IBusConfig
    {
        public int WriterLockTimeout => 60000;
        public int ReaderLockTimeout => 60000;
    }
}
