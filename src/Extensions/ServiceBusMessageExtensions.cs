using System;
using System.Text;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using Newtonsoft.Json;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public static class ServiceBusMessageExtensions
{
    public const string EVENT_TYPE = "ContainedEventType";

    /// <summary>
    /// Converts a ServiceBusMessage to a DomainEvent
    /// </summary>
    /// <param name="serviceBusMessage"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If UserProperty does not contain a Event Type property</exception>
    /// <exception cref="ArgumentException">If Event Type not available in solution</exception>
    public static IDomainEvent ToDomainEvent(this ServiceBusMessage serviceBusMessage)
    {
        if (serviceBusMessage.ApplicationProperties.ContainsKey(EVENT_TYPE) == false)
        {
            throw new ArgumentException($"Message does not contain UserProperty '{EVENT_TYPE}'");
        }

        string eventTypeName = serviceBusMessage.ApplicationProperties[EVENT_TYPE].ToString();

        if (eventTypeName == null)
        {
            throw new ArgumentException($"Can't find Event Type in ApplicationProperty: {EVENT_TYPE}");
        }
            
        Type eventType = Type.GetType(eventTypeName, false, true);

        if (eventType == null)
        {
            throw new ArgumentException($"Can't find Event Type: {eventTypeName} in current solution.");
        }

        string messageAsString = Encoding.UTF8.GetString(serviceBusMessage.Body);

        return JsonConvert.DeserializeObject(messageAsString, eventType) as IDomainEvent;
    }
}