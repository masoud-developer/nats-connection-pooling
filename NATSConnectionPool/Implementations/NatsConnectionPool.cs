using NATSConnectionPool.Interfaces;

namespace NATSConnectionPool.Implementations;

internal class NatsConnectionPool : Pool<INatsPooledConnection>, INatsConnectionPool
{
    public NatsConnectionPool(int size, Func<Pool<INatsPooledConnection>, INatsPooledConnection> factory) : base(size, factory)
    {
    }
}