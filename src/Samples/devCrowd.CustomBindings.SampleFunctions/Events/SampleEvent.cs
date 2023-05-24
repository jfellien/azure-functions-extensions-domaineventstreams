using devCrowd.CustomBindings.EventSourcing;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.SampleFunctions.Events;


public class SampleEvent : DomainEvent
{
    public SampleEvent(string requesterId) : base(requesterId)
    {
    }


    public string EntityId { get; set; }
    public string EntityName { get; set; }
}