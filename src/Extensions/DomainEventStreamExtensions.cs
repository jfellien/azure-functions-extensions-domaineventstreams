using System.Collections.Generic;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;

namespace AzureFunctions.Extensions.EventSourcing.Extensions
{
    public static class DomainEventStreamExtensions
    {
        public static GetSingleDomainEventFilter<T> Get<T>(this IEnumerable<object> domainEvents) 
            where T : IDomainEvent
        {
            return new GetSingleDomainEventFilter<T>(domainEvents);
        }

        public static GetManyDomainEventFilter<T> GetMany<T>(this IEnumerable<object> domainEvents)
            where T : IDomainEvent
        {
            return new GetManyDomainEventFilter<T>(domainEvents);
        }

        public static GetFirstSingleDomainEventFilter<T> GetFirst<T>(this IEnumerable<object> domainEvents) 
            where T : IDomainEvent
        {
            return new GetFirstSingleDomainEventFilter<T>(domainEvents);
        }
        
        public static GetLastSingleDomainEventFilter<T> GetLast<T>(this IEnumerable<object> domainEvents) 
            where T : IDomainEvent
        {
            return new GetLastSingleDomainEventFilter<T>(domainEvents);
        }

        public static DomainEventChecks<T> Event<T>(this IEnumerable<object> domainEvents) where T : IDomainEvent
        {
            return new DomainEventChecks<T>(domainEvents);
        }
        
        public static DomainEventStreamQueries<T> Event<T>(this IEnumerable<object> domainEvents, T domainEvent) 
            where T : IDomainEvent
        {
            return new DomainEventStreamQueries<T>(domainEvents, domainEvent);
        }
    }
}