namespace Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages
{
    public interface IDomainEvent
    {
        DomainEventHeader Header { get; set; }
    }
}