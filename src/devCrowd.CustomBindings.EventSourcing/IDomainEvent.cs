namespace devCrowd.CustomBindings.EventSourcing;

public interface IDomainEvent
{
    DomainEventHeader Header { get; set; }
}