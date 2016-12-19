namespace MemoryBus
{
    public interface IBusConfig
    {
        int ReaderLockTimeout { get; }
        int WriterLockTimeout { get; }
    }
}
