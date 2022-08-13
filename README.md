# nats-connection-pooling
This repository help to you have a connection pool of your dotnet application NATS connections.

## Installation

1- Install [NATSConnectionPool](https://www.nuget.org/packages/NATSConnectionPool/1.0.0) nuget package.

2- In your startup class config method add bellow section
```bash
  services.AddNatsConnectionPool(3, () => 
  {
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
        return opt;
  });
```
3- Inject connection pool in your classes
```bash
public class FooBusiness()
{
    private readonly INatsConnectionPool _pool;
    public NatsListener(INatsConnectionPool pool)
    {
        _pool = pool;
    }


    public void FooMethod()
    {
        using(var connection = _pool.Acquire())
        {
            ...
            your code is here
            ...
        }
    }
}
```
## 🛠 Skills
C# Dotnet

## Support

For support, email masoud_shafaghi@outlook.com or join our Slack channel.