using System.Collections.Generic;

namespace AzureFunctions.Extensions.EventSourcing.EventStreamStorages
{
    public class DomainEventSequence : List<SequencedDomainEvent>{}
}