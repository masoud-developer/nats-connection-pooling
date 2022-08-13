namespace NATSConnectionPool.Interfaces;

public interface IPool<TItem>
{
    TItem Acquire();
    void Release(TItem item);
    bool IsDisposed { get; }
}