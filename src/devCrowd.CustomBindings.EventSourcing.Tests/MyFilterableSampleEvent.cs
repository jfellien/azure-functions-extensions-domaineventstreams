using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Tests;

public class MyFilterableSampleEvent :DomainEvent
{
    public MyFilterableSampleEvent(string requesterId) : base(requesterId)
    {
        FilterableValue = string.Empty;
    }

    public string FilterableValue { get; set; }
}