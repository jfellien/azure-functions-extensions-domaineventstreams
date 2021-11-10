using System.Collections.Generic;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    public class DomainEventSequence : List<SequencedDomainEvent>
    {
        public bool HasBeenSequenced { get; internal set; }
    }
}