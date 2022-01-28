using System.Text;
using Azure.Messaging.ServiceBus;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using Newtonsoft.Json;

#nullable enable

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
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
            var eventAsString = JsonConvert.SerializeObject(domainEvent);
            var eventAsBytes = Encoding.UTF8.GetBytes(eventAsString);

            var serviceBusMessage = new ServiceBusMessage(eventAsBytes);
            var eventTypeName = domainEvent.GetType().AssemblyQualifiedName;

            serviceBusMessage.ApplicationProperties.Add(ServiceBusMessageExtensions.EVENT_TYPE, eventTypeName);

            return serviceBusMessage;
        }
    }
}