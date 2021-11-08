using System.Threading.Tasks;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;
using AzureFunctions.Extensions.EventSourcing.Extensions;
using Microsoft.Azure.ServiceBus;

namespace AzureFunctions.Extensions.EventSourcing.EventsPublisher
{
    internal class ServiceBusDomainEventsPublisher : IPublishDomainEvents
    {
        private readonly TopicClient _topicClient;

        public ServiceBusDomainEventsPublisher(string connectionString, string topic)
        {
            _topicClient = new TopicClient(connectionString, topic);
        }
        
        public Task Publish(IDomainEvent domainEvent)
        {
            var eventAsMessage = domainEvent.ToServiceBusMessage();

            return _topicClient.SendAsync(eventAsMessage);
        }
    }
}