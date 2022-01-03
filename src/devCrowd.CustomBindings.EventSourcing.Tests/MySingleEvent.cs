using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class MySingleEvent : DomainEvent
{
    public MySingleEvent(string requesterId) : base(requesterId)
    {
    }
}