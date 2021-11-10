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
        private readonly string _dbName;
        private readonly string _tableName;
        private readonly SqlConnection _connection;
        private long _lastSequenceNumberOfStream;
        private bool _domainEventStreamHasBeenRead;
        private readonly JsonSerializerSettings _defaultSerializerSettings;

        public SqlServerDomainEventStreamStorage(string connectionString, string dbName, string tableName)
        {
            _dbName = dbName;
            _tableName = tableName;
            _connection = new SqlConnection(connectionString);
            
            _defaultSerializerSettings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };
        }
        
        public Task<DomainEventSequence> ReadBy(string context, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE context=@context " +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }", _connection);

            query.Parameters.AddWithValue("@context", context.ToLower());

            return ReadDomainEventStream(query, cancellationToken);
        }

        public Task<DomainEventSequence> ReadBy(string context, string entity, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE { SqlServerDomainEventStreamStorageColumnNames.Context }=@context " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.Entity }=@entity" +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }", _connection);
            
            query.Parameters.AddWithValue("@context", context.ToLower());
            query.Parameters.AddWithValue("@entity", entity.ToLower());

            return ReadDomainEventStream(query, cancellationToken);
        }

        public Task<DomainEventSequence> ReadBy(string context, string entity, string entityId, CancellationToken cancellationToken)
        {
            var query = new SqlCommand(
                $"SELECT * FROM [{_tableName}] " +
                $"WHERE { SqlServerDomainEventStreamStorageColumnNames.Context }=@context " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.Entity }=@entity " +
                $"AND { SqlServerDomainEventStreamStorageColumnNames.EntityId }=@entityId " +
                $"ORDER BY { SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }", _connection);
            
            query.Parameters.AddWithValue("@context", context.ToLower());
            query.Parameters.AddWithValue("@entity", entity.ToLower());
            query.Parameters.AddWithValue("@entityId", entityId);

            return ReadDomainEventStream(query, cancellationToken);
        }

        public async Task<long> Write(IDomainEvent domainEvent, string context, string entity = null, string entityId = null)
        {
            var insertQuery =
                $"INSERT INTO [{_tableName}] (" +
                $"{ SqlServerDomainEventStreamStorageColumnNames.EventId }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.Context }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.EntityId }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.Entity }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.EventName }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.EventFullName }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.IsoTimeStamp }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.SequenceNumber }, " +
                $"{ SqlServerDomainEventStreamStorageColumnNames.PayLoad }) " +
                $"VALUES (@eventId, @context, @entityId, @entity, @eventName, @eventFullName, @isoTimeStamp, @sequenceNumber, @payload)";
            
            await using var command = new SqlCommand(insertQuery, _connection);
            
            var serializedDomainEvent = JsonConvert.SerializeObject(domainEvent);
            var nextSequenceNumber = await NextSequenceNumber(context, entity, entityId);
                
            command.Parameters.AddWithValue("@eventId", Guid.NewGuid().ToString());
            command.Parameters.AddWithValue("@context", context.ToLowerInvariant());
            command.Parameters.AddWithValue("@entity", entity?.ToLowerInvariant());
            command.Parameters.AddWithValue("@entityId", entityId);
            command.Parameters.AddWithValue("@eventName", domainEvent.GetType().Name);
            command.Parameters.AddWithValue("@eventFullName", domainEvent.GetType().AssemblyQualifiedName);
            command.Parameters.AddWithValue("@isoTimestamp", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("@sequenceNumber", nextSequenceNumber);
            command.Parameters.AddWithValue("@payload", serializedDomainEvent);

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
            
            await command.Connection.OpenAsync(cancellationToken);

            try
            {
                var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    events.Add(GetDomainEventFromReader(reader));
                }

                _lastSequenceNumberOfStream = events.Any()
                    ? events.Last().SequenceNumber
                    : 0;

                _domainEventStreamHasBeenRead = true;
            }
            finally
            {
                await command.Connection.CloseAsync();
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

            await using var command = new SqlCommand(query,_connection);
            
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