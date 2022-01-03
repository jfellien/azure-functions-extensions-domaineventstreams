using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class MyNotExistingEvent : DomainEvent
{
    public MyNotExistingEvent(string requesterId) : base(requesterId)
    {
    }
}