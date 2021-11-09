using Newtonsoft.Json;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    internal class LastSequenceNumberQueryResult
    {
        [JsonProperty("maxSequenceNumber")] 
        public long MaxSequenceNumber { get; set; }
    }
}