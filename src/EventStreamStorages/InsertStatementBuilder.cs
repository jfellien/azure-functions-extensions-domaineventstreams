using System.Text;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    internal class InsertStatementBuilder
    {
        internal static string ColumnList(string entity = null, string entityId = null)
        {
            var columnList = new StringBuilder(
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

        internal static string ParameterList(string entity = null, string entityId = null)
        {
            var parameterList = new StringBuilder("@eventId, @context, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload");

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
}