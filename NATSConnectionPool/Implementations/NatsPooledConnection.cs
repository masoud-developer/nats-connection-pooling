using System.Text;
using NATS.Client;
using NATSConnectionPool.Interfaces;

namespace NATSConnectionPool.Implementations;

internal class NatsPooledConnection : INatsPooledConnection, IDisposable
{
    private readonly IPool<INatsPooledConnection> _pool;
    public ConnectionFactory Factory { get; }
    public IConnection Connection { get; private set; }

    public NatsPooledConnection(IPool<INatsPooledConnection> pool, string user, string pass, params string[] servers)
    {
        _pool = pool;
        Factory = new ConnectionFactory();
        var opt = ConnectionFactory.GetDefaultOptions();
        opt.AllowReconnect = true;
        opt.MaxReconnect = 10;
        opt.ReconnectWait = 20;
        opt.User = user;
        opt.Password = pass;
        opt.Servers = servers;

        opt.AsyncErrorEventHandler += (sender, args) =>
        {
            var strBuilder = new StringBuilder("Error: ");
            strBuilder.AppendLine("   Server: " + args.Conn.ConnectedUrl);
            strBuilder.AppendLine("   Message: " + args.Error);
            strBuilder.AppendLine("   Subject: " + args.Subscription.Subject);
            strBuilder.AppendLine("   Queue: " + args.Subscription.Queue);
            Console.WriteLine(strBuilder.ToString());
        };

        opt.ServerDiscoveredEventHandler += (sender, args) =>
        {
            var strBuilder = new StringBuilder("A new server has joined the cluster:");
            strBuilder.AppendLine("    " + string.Join(", ", args.Conn.DiscoveredServers));
            Console.WriteLine(strBuilder.ToString());
        };

        opt.ClosedEventHandler += (sender, args) =>
        {
            var strBuilder = new StringBuilder("Connection Closed: ");
            strBuilder.AppendLine("   Server: " + args.Conn.ConnectedUrl);
            Console.WriteLine(strBuilder.ToString());
        };

        opt.DisconnectedEventHandler += (sender, args) =>
        {
            var strBuilder = new StringBuilder("Connection Disconnected: ");
            strBuilder.AppendLine("   Server: " + args.Conn.ConnectedUrl);
            Console.WriteLine(strBuilder.ToString());
        };

        Connection = Factory.CreateConnection(opt);
    }

    public NatsPooledConnection(IPool<INatsPooledConnection> pool, Func<Options> getOption)
    {
        _pool = pool;
        Factory = new ConnectionFactory();
        Connection = Factory.CreateConnection(getOption());
    }

    public async Task Close()
    {
        Connection?.Close();
        Connection?.Dispose();
        Connection = null;
    }

    public void Dispose()
    {
        if (_pool.IsDisposed)
        {
            if (Connection == null)
                return;
            Connection?.Close();
            Connection?.Dispose();
            Connection = null;
        }
        else
            _pool.Release(this);
    }
}