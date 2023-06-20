using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.Extensions;

namespace devCrowd.CustomBindings.EventSourcing.EventsPublisher;

public class ServiceBusDomainEventsPublisher : IPublishDomainEvents
{
    private readonly string _connectionString;
    private readonly string _topic;

    public ServiceBusDomainEventsPublisher(string connectionString, string topic)
    {
        _connectionString = connectionString;
        _topic = topic;
    }
        
    public async Task Publish(IDomainEvent domainEvent)
    {
        await using ServiceBusClient client = GetFromConnection(_connectionString);

        ServiceBusSender sender = client.CreateSender(_topic);
            
        ServiceBusMessage eventAsMessage = domainEvent.ToServiceBusMessage();
            
        await sender.SendMessageAsync(eventAsMessage);
    }

    private ServiceBusClient GetFromConnection(string connection)
    {
        if (IsServiceBusNamespace(connection))
        {
            return new ServiceBusClient(connection, new DefaultAzureCredential());
        }

        return new ServiceBusClient(connection);
    }

    private static bool IsServiceBusNamespace(string connection)
    {
        return connection.EndsWith(".servicebus.windows.net")
               && connection.Contains("Endpoint=sb://") == false;
    }
}