using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using devCrowd.CustomBindings.EventSourcing.Extensions;
using Azure.Messaging.ServiceBus;

namespace devCrowd.CustomBindings.EventSourcing.EventsPublisher
{
    public class ServiceBusDomainEventsPublisher : IPublishDomainEvents
    {
        private readonly ServiceBusSender _topicClient;

        public ServiceBusDomainEventsPublisher(string connectionString, string topic)
        {
            var serviceBusClient = new ServiceBusClient(connectionString);
            _topicClient = serviceBusClient.CreateSender(topic);
        }
        
        public Task Publish(IDomainEvent domainEvent)
        {
            var eventAsMessage = domainEvent.ToServiceBusMessage();

            return _topicClient.SendMessageAsync(eventAsMessage);
        }
    }
}