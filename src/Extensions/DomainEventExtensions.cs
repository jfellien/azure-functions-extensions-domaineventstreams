using System.Text;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

#nullable enable

namespace AzureFunctions.Extensions.EventSourcing.Extensions
{
    public static class DomainEventExtensions
    {
        /// <summary>
        /// Converts a DomainEvent to a ServiceBusMessage and sets
        /// a UserProperty with EventType Name
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        public static Message ToServiceBusMessage(this IDomainEvent domainEvent)
        {
            var eventAsString = JsonConvert.SerializeObject(domainEvent);
            var eventAsBytes = Encoding.UTF8.GetBytes(eventAsString);

            var serviceBusMessage = new Message(eventAsBytes);
            var eventTypeName = domainEvent.GetType().AssemblyQualifiedName;

            serviceBusMessage.UserProperties.Add(ServiceBusMessageExtensions.EVENT_TYPE, eventTypeName);

            return serviceBusMessage;
        }
    }
}