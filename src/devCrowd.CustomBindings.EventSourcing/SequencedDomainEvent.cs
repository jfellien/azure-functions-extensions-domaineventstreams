namespace devCrowd.CustomBindings.EventSourcing;

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