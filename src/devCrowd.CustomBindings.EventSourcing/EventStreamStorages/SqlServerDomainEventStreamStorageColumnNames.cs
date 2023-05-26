namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

internal static class SqlServerDomainEventStreamStorageColumnNames
{
    public const string EventId = "EventId";
    public const string EventName = "EventName";
    public const string EventFullName = "EventFullName";
    public const string IsoTimeStamp = "IsoTimeStamp";
    public const string SequenceNumber = "SequenceNumber";
    public const string Context = "Context";
    public const string Entity = "Entity";
    public const string EntityId = "EntityId";
    public const string PayLoad = "PayLoad";
}