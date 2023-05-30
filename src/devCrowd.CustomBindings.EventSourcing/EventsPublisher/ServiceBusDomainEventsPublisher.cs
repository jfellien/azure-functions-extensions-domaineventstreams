using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.Extensions;

namespace devCrowd.CustomBindings.EventSourcing.EventsPublisher;

public class ServiceBusDomainEventsPublisher : IPublishDomainEvents
{
    private const string SERVICE_BUS_NAMESPACE = "servicebus.windows.net";
    
    private readonly string _connection;
    private readonly string _topic;

    public ServiceBusDomainEventsPublisher(string connection, string topic)
    {
        _connection = connection;
        _topic = topic;
    }
        
    public async Task Publish(IDomainEvent domainEvent)
    {
        await using ServiceBusClient client = GetClient();

        ServiceBusSender sender = client.CreateSender(_topic);
            
        ServiceBusMessage eventAsMessage = domainEvent.ToServiceBusMessage();
            
        await sender.SendMessageAsync(eventAsMessage);
    }

    private ServiceBusClient GetClient()
    {
        return _connection.EndsWith(SERVICE_BUS_NAMESPACE) 
            ? new ServiceBusClient(_connection, new DefaultAzureCredential()) 
            : new ServiceBusClient(_connection);
    }
}