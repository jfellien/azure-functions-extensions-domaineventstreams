namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    public interface IDomainEvent
    {
        DomainEventHeader Header { get; set; }
    }
}