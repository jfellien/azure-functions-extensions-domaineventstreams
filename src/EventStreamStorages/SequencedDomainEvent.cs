namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    public class SequencedDomainEvent
    {
        public SequencedDomainEvent(long sequenceNumber, IDomainEvent instance)
        {
            SequenceNumber = sequenceNumber;
            Instance = instance;
        }
        
        public long SequenceNumber { get; }
        public IDomainEvent Instance { get; }
    }
}