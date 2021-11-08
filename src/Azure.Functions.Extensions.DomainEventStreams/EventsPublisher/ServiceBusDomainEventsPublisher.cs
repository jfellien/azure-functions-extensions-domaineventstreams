using System.Threading.Tasks;
using Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages;
using Azure.Functions.Extensions.DomainEventStreams.Extensions;
using Microsoft.Azure.ServiceBus;

namespace Azure.Functions.Extensions.DomainEventStreams.EventsPublisher
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