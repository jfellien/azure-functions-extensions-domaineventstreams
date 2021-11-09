using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.EventsPublisher
{
    public interface IPublishDomainEvents
    {
        Task Publish(IDomainEvent domainEvent);
    }
}