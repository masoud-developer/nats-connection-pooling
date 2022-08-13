using NATS.Client;

namespace NATSConnectionPool.Interfaces;

public interface INatsPooledConnection : IDisposable
{
    ConnectionFactory Factory { get; }
    IConnection Connection { get; }
}