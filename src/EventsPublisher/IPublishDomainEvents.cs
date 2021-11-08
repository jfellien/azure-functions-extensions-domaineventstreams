using System.Threading.Tasks;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;

namespace AzureFunctions.Extensions.EventSourcing.EventsPublisher
{
    public interface IPublishDomainEvents
    {
        Task Publish(IDomainEvent domainEvent);
    }
}