using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages
{
    public class SqlServerDomainEventStreamStorage : IReadAndWriteDomainEvents
    {
        private readonly string _connectionString;
        private readonly string _dbName;
        private readonly string _tableName;
        private long _lastSequenceNumberOfStream;
        private bool _domainEventStreamHasBeenRead;
        private readonly JsonSerializerSettings _defaultSerializerSettings;

        public SqlServerDomainEventStreamStorage(string connectionString, string dbName, string tableName)
        {
            _connectionString = connectionString;
            _dbName = dbName;
            _tableName = tableName;

            _defaultSerializerSettings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };
        }

        public async Task<DomainEventSequence> ReadBy(string context, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE context=@context " +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }");

            query.Parameters.AddWithValue("@context", context.ToLower());

            return await ReadDomainEventStream(query, cancellationToken);
        }

        public async Task<DomainEventSequence> ReadBy(string context, string entity, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE { SqlServerDomainEventStreamStorageColumnNames.Context }=@context " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.Entity }=@entity " +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }");

            query.Parameters.AddWithValue("@context", context.ToLower());
            query.Parameters.AddWithValue("@entity", entity.ToLower());

            return await ReadDomainEventStream(query, cancellationToken);
        }

        public async Task<DomainEventSequence> ReadBy(string context, string entity, string entityId, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE { SqlServerDomainEventStreamStorageColumnNames.Context }=@context " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.Entity }=@entity " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.EntityId }=@entityId " +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }");

            query.Parameters.AddWithValue("@context", context.ToLower());
            query.Parameters.AddWithValue("@entity", entity.ToLower());
            query.Parameters.AddWithValue("@entityId", entityId);

            return await ReadDomainEventStream(query, cancellationToken);
        }

        public async Task<long> Write(IDomainEvent domainEvent, string context, string entity = null, string entityId = null)
        {
            var insertQuery = InsertStatementBuilder.GetStatement(_tableName, entity, entityId);
            
            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(insertQuery, connection);

            var serializedDomainEvent = JsonConvert.SerializeObject(domainEvent);
            var nextSequenceNumber = await NextSequenceNumber(context, entity, entityId);

            command.Parameters.AddWithValue("@eventId", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@context", context.ToLowerInvariant());
            command.Parameters.AddWithValue("@eventName", domainEvent.GetType().Name);
            command.Parameters.AddWithValue("@eventFullName", domainEvent.GetType().AssemblyQualifiedName);
            command.Parameters.AddWithValue("@isoTimestamp", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@sequenceNumber", nextSequenceNumber);
            command.Parameters.AddWithValue("@payload", serializedDomainEvent);
            
            if (string.IsNullOrEmpty(entity) == false)
            {
                command.Parameters.AddWithValue("@entity", entity.ToLowerInvariant());
            }

            if (string.IsNullOrEmpty(entityId) == false)
            {
                command.Parameters.AddWithValue("@entityId", entityId);
            }

            try
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                command.ExecuteNonQuery();
            }
            finally
            {
                await command.Connection.CloseAsync();
            }

            return nextSequenceNumber;
        }

        private async Task<DomainEventSequence> ReadDomainEventStream(SqlCommand command, CancellationToken cancellationToken)
        {
            var events = new DomainEventSequence();

            events.HasBeenSequenced = true;

            try
            {
                using SqlConnection connection = new SqlConnection(_connectionString);

                command.Connection = connection;
                connection.Open();

                var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    events.Add(GetDomainEventFromReader(reader));
                }

                reader.Close();

                _lastSequenceNumberOfStream = events.Any()
                    ? events.Last().SequenceNumber
                    : 0;

                _domainEventStreamHasBeenRead = true;
            }
            finally
            {
                command.Connection.Close();
            }

            return events;
        }

        private SequencedDomainEvent GetDomainEventFromReader(SqlDataReader reader)
        {
            var eventType = reader.GetString(SqlServerDomainEventStreamStorageColumnNames.EventFullName);
            var domainEventType = Type.GetType(eventType);

            if (domainEventType == null)
            {
                throw new TypeAccessException(
                    $"Deserialization Error: Event Type {eventType} is not part of Events anymore");
            }

            var domainEventAsString = reader.GetString(SqlServerDomainEventStreamStorageColumnNames.PayLoad);
            var domainEventInstance =
                JsonConvert.DeserializeObject(domainEventAsString, domainEventType, _defaultSerializerSettings) as IDomainEvent;


            var sequenceNumber = reader.GetInt64(SqlServerDomainEventStreamStorageColumnNames.SequenceNumber);

            var sequencedDomainEvent = new SequencedDomainEvent(sequenceNumber, domainEventInstance);

            return sequencedDomainEvent;
        }

        private async Task<long> NextSequenceNumber(string context, string entity, string entityId)
        {
            // If we don't have read the stream already,
            // first we need the last sequence number from database
            // by get the MAX value
            if (_domainEventStreamHasBeenRead == false)
            {
                _lastSequenceNumberOfStream = await GetLastSequenceNumberOfStream(context, entity, entityId);
            }

            // I don't know why, but a simple _lastSequenceNumberOfStream++ doesn't work
            _lastSequenceNumberOfStream = _lastSequenceNumberOfStream + 1;

            return _lastSequenceNumberOfStream;
        }

        private async Task<long> GetLastSequenceNumberOfStream(string context, string entity, string entityId)
        {
            var query = BuildQueryStringForLastSequenceNumber(context, entity, entityId);

            using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@context", context.ToLower());

            if (string.IsNullOrWhiteSpace(entity) == false)
            {
                command.Parameters.AddWithValue("@entity", entity.ToLower());
            }

            if (string.IsNullOrWhiteSpace(entityId) == false)
            {
                command.Parameters.AddWithValue("@entityId", entityId.ToLower());
            }

            try
            {
                await command.Connection.OpenAsync();

                var scalarResult = await command.ExecuteScalarAsync();

                var lastSequenceNumber = scalarResult is DBNull ? 0 : Convert.ToInt64(scalarResult);

                return lastSequenceNumber;

            }
            finally
            {
                await command.Connection.CloseAsync();
            }

        }

        private string BuildQueryStringForLastSequenceNumber(string context, string entity, string entityId)
        {
            var queryString =
                $"SELECT MAX({ SqlServerDomainEventStreamStorageColumnNames.SequenceNumber }) AS maxSequenceNumber " +
                $"FROM [{_tableName}] " +
                $"WHERE {SqlServerDomainEventStreamStorageColumnNames.Context}=@context";

            if (string.IsNullOrWhiteSpace(entity) == false)
            {
                queryString += $" AND { SqlServerDomainEventStreamStorageColumnNames.Entity }=@entity";
            }

            if (string.IsNullOrWhiteSpace(entityId) == false)
            {
                queryString += $" AND { SqlServerDomainEventStreamStorageColumnNames.EntityId }=@entityId";
            }

            return queryString;
        }
    }
}