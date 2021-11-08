using System;

namespace AzureFunctions.Extensions.EventSourcing.EventStreamStorages
{
    public abstract class DomainEvent : IDomainEvent
    {
        private DomainEvent()
        {
            Header = new DomainEventHeader
            {
                TracingId = Guid.NewGuid().ToString()
            };
        }

        protected DomainEvent(string requesterId) : this()
        {
            Header.RequesterId = requesterId;
        }
        
        public DomainEventHeader Header { get; set; }
    }
}