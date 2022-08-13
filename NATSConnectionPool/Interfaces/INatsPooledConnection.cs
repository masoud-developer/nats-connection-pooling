namespace NATSConnectionPool.Interfaces;

public interface IQueueClient : IDisposable
{
    Task Enqueue(string queueName, byte[] data);
    Task Enqueue(string queueName, object data);
    object AddSubscriber(string queueName, EventHandler<object> handler);
    Task Close();
}