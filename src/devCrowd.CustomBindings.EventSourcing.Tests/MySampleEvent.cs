using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class MySampleEvent : DomainEvent
{
    public MySampleEvent(string requesterId) : base(requesterId)
    {
    }
}