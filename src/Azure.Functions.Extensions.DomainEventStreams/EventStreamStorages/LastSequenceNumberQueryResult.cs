using Newtonsoft.Json;

namespace Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages
{
    internal class LastSequenceNumberQueryResult
    {
        [JsonProperty("maxSequenceNumber")] 
        public long MaxSequenceNumber { get; set; }
    }
}