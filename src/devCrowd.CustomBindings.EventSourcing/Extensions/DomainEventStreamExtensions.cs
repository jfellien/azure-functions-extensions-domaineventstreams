using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public static class DomainEventStreamExtensions
{
    public static DomainEventsFilter<T> Get<T>(this IEnumerable<object> domainEvents) 
        where T : IDomainEvent
    {
        return new DomainEventsFilter<T>(domainEvents);
    }

    public static DomainEventChecks<T> Event<T>(this IEnumerable<object> domainEvents) 
        where T : IDomainEvent
    {
        return new DomainEventChecks<T>(domainEvents);
    }
}