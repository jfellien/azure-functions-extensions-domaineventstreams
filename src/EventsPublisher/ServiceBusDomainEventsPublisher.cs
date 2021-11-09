using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using devCrowd.CustomBindings.EventSourcing.Extensions;
using Microsoft.Azure.ServiceBus;

namespace devCrowd.CustomBindings.EventSourcing.EventsPublisher
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