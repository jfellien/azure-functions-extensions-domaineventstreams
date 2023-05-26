using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
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
        await using ServiceBusClient client = new(_connectionString);

        ServiceBusSender sender = client.CreateSender(_topic);
            
        ServiceBusMessage eventAsMessage = domainEvent.ToServiceBusMessage();
            
        await sender.SendMessageAsync(eventAsMessage);
    }
}