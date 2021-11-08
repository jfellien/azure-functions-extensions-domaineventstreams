namespace Azure.Functions.Extensions.DomainEventStreams
{
    public class DomainEventHeader
    {
        public string RequesterId { get; set; }
        public string TracingId { get; set; }
    }
}