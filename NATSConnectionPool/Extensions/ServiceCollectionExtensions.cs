using Microsoft.Extensions.DependencyInjection;
using NATS.Client;
using NATSConnectionPool.Implementations;
using NATSConnectionPool.Interfaces;

namespace NATSConnectionPool.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddNatsConnectionPool(this IServiceCollection serviceCollection, int poolSize, string user,
        string pass, params string[] servers)
    {
        serviceCollection.AddSingleton(typeof(INatsConnectionPool),
            new Pool<INatsPooledConnection>(poolSize, p => new NatsPooledConnection(p, user, pass, servers)));
    }

    public static void AddNatsConnectionPool(this IServiceCollection serviceCollection, int poolSize,
        Func<Options> getOption)
    {
        serviceCollection.AddSingleton(typeof(INatsConnectionPool),
            new Pool<INatsPooledConnection>(poolSize, p => new NatsPooledConnection(p, getOption)));
    }
}