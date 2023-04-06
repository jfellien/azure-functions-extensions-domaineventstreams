using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

#nullable enable

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public static class DomainEventExtensions
{
    /// <summary>
    /// Converts a DomainEvent to a ServiceBusMessage and sets
    /// a UserProperty with EventType Name
    /// </summary>
    /// <param name="domainEvent"></param>
    /// <returns></returns>
    public static ServiceBusMessage ToServiceBusMessage(this IDomainEvent domainEvent)
    {
        string eventAsString = JsonSerializer.Serialize(domainEvent);
        byte[] eventAsBytes = Encoding.UTF8.GetBytes(eventAsString);

        ServiceBusMessage serviceBusMessage = new (eventAsBytes);
        string? eventTypeName = domainEvent.GetType().AssemblyQualifiedName;

        serviceBusMessage.ApplicationProperties.Add(ServiceBusMessageExtensions.EVENT_TYPE, eventTypeName);

        return serviceBusMessage;
    }
}