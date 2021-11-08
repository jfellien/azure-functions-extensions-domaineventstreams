namespace AzureFunctions.Extensions.EventSourcing.EventStreamStorages
{
    public interface IDomainEvent
    {
        DomainEventHeader Header { get; set; }
    }
}