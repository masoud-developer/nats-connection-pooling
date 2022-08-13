using MicroEthos.Common.Contracts;
using MicroEthos.Common.Implementation;
using MicroEthos.Common.Utils.DataStructure;
using Microsoft.Extensions.DependencyInjection;

namespace MicroEthos.Common.Utils.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddNatsConnectionPool(this IServiceCollection serviceCollection, int poolSize, string user,
        string pass, params string[] servers)
    {
        serviceCollection.AddSingleton(typeof(IPool<IQueueClient>), serviceProvider =>
        {
            return new Pool<IQueueClient>(poolSize, p =>
            {
                var scope = serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<NatsClient>>();
                return new NatsPooledConnection(p, logger, user, pass, servers);
            });
        });
    }

    /// <summary>
    /// Add nats client with scoped registration
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="user"></param>
    /// <param name="pass"></param>
    /// <param name="servers"></param>
    public static void AddNats(this IServiceCollection serviceCollection, string user,
        string pass, params string[] servers)
    {
        serviceCollection.AddScoped(typeof(IQueueClient), serviceProvider =>
        {
            var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<NatsClient>>();
            return new NatsClient(logger, user, pass, servers);
        });
    }
}