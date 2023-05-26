using System.Text;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

internal class InsertStatementBuilder
{
    public static string GetStatement(string tableName, string entity = null, string entityId = null)
    {
        return $"INSERT INTO [{tableName}] " + 
               $"({ ColumnList(entity, entityId) }) " + 
               $"VALUES ({ ParameterList(entity, entityId)})";
    }

    private static string ColumnList(string entity = null, string entityId = null)
    {
        StringBuilder columnList = new StringBuilder(
            $"{SqlServerDomainEventStreamStorageColumnNames.EventId}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.Context}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.EventFullName}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.SequenceNumber}, " +
            $"{SqlServerDomainEventStreamStorageColumnNames.PayLoad}");

        if (string.IsNullOrEmpty(entity) == false)
        {
            columnList.Append($", {SqlServerDomainEventStreamStorageColumnNames.Entity}");
        }

        if (string.IsNullOrEmpty(entityId) == false)
        {
            columnList.Append($", {SqlServerDomainEventStreamStorageColumnNames.EntityId}");
        }

        return columnList.ToString();
    }

    private  static string ParameterList(string entity = null, string entityId = null)
    {
        StringBuilder parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload");

        if (string.IsNullOrEmpty(entity) == false)
        {
            parameterList.Append(", @entity");
        }

        if (string.IsNullOrEmpty(entityId) == false)
        {
            parameterList.Append(", @entityId");
        }

        return parameterList.ToString();
    }
}