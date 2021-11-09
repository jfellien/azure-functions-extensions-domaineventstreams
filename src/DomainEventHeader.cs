namespace devCrowd.CustomBindings.EventSourcing
{
    public class DomainEventHeader
    {
        public string RequesterId { get; set; }
        public string TracingId { get; set; }
    }
}