using System.Threading.Tasks;
using Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages;

namespace Azure.Functions.Extensions.DomainEventStreams.EventsPublisher
{
    public interface IPublishDomainEvents
    {
        Task Publish(IDomainEvent domainEvent);
    }
}