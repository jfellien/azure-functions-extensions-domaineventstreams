using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

public class CosmosDbDomainEventStreamStorage : IReadAndWriteDomainEvents
{
    private readonly JsonSerializer _defaultSerializer;
    private readonly Container _domainEventsContainer;

    private bool _domainEventStreamHasBeenRead;
    private long _lastSequenceNumberOfStream;

    public static CosmosDbDomainEventStreamStorage CreateFromConnectionString(
        string connectionString,
        string dbName,
        string collectionName)
    {
        CosmosClient client = new(connectionString);
        
        return new CosmosDbDomainEventStreamStorage(client, dbName, collectionName);
    }
    
    public static CosmosDbDomainEventStreamStorage CreateFromServiceEndpoint(
        string serviceEndpoint,
        string dbName,
        string collectionName)
    {
        CosmosClient client = new(serviceEndpoint, new DefaultAzureCredential());
        
        return new CosmosDbDomainEventStreamStorage(client, dbName, collectionName);
    }

    private CosmosDbDomainEventStreamStorage(CosmosClient cosmosClient, string dbName, string collectionName)
    {
        _domainEventsContainer = cosmosClient.GetContainer(dbName, collectionName);

        JsonSerializerSettings serializerSettings = new ()
        {
            DateParseHandling = DateParseHandling.None
        };

        _defaultSerializer = JsonSerializer.CreateDefault(serializerSettings);
    }
    
    
    public Task<DomainEventSequence> ReadBy(string context, CancellationToken cancellationToken)
    {
        QueryDefinition query = new QueryDefinition(
                "SELECT * FROM c " +
                "WHERE c.context=@context " +
                "ORDER BY c.isoTimestamp")
            .WithParameter("@context", context.ToLowerInvariant());

        return ReadDomainEventsStream(query, cancellationToken);
    }

    public Task<DomainEventSequence> ReadBy(string context, string entity, CancellationToken cancellationToken)
    {
        QueryDefinition query = new QueryDefinition(
                "SELECT * FROM c " +
                "WHERE c.context=@context " +
                "AND c.entity=@entity " +
                "ORDER BY c.isoTimestamp")
            .WithParameter("@context", context.ToLowerInvariant())
            .WithParameter("@entity", entity.ToLowerInvariant());

        return ReadDomainEventsStream(query, cancellationToken);
    }

    public Task<DomainEventSequence> ReadBy(string context, string entity, string entityId, CancellationToken cancellationToken)
    {
        QueryDefinition query = new QueryDefinition(
                "SELECT * FROM c " +
                "WHERE c.context=@context " +
                "AND c.entity=@entity " +
                "AND c.entityId=@entityId " +
                "ORDER BY c.isoTimestamp")
            .WithParameter("@context", context.ToLowerInvariant())
            .WithParameter("@entity", entity.ToLowerInvariant())
            .WithParameter("@entityId", entityId);

        return ReadDomainEventsStream(query, cancellationToken);
    }

    public async Task<long> Write(IDomainEvent domainEvent, string context, string entity = null, string entityId = null)
    {
        DomainEventWrap domainEventWrap = new DomainEventWrap
        {
            EventId = Guid.NewGuid().ToString(),

            Context = context.ToLowerInvariant(),
            Entity = entity?.ToLowerInvariant(),
            EntityId = entityId,

            EventName = domainEvent.GetType().Name,
            EventFullName = domainEvent.GetType().AssemblyQualifiedName,
            IsoTimestamp = DateTime.UtcNow.ToString("O"),
            SequenceNumber = await NextSequenceNumber(context, entity, entityId),

            DomainEvent = domainEvent
        };

        await _domainEventsContainer.CreateItemAsync(domainEventWrap);

        return domainEventWrap.SequenceNumber;
    }

    private async Task<DomainEventSequence> ReadDomainEventsStream(QueryDefinition query, CancellationToken cancellationToken)
    {
        DomainEventSequence events = new DomainEventSequence();
            
        events.HasBeenSequenced = true;
            
        using (FeedIterator<JObject> resultSet = _domainEventsContainer.GetItemQueryIterator<JObject>(query))
        {
            while (resultSet.HasMoreResults)
            {
                FeedResponse<JObject> wrappedDomainEvents = await resultSet.ReadNextAsync(cancellationToken);

                IEnumerable<SequencedDomainEvent> sequencedDomainEvents = wrappedDomainEvents.Select(ConvertFromEventData);

                events.AddRange(sequencedDomainEvents);

                _lastSequenceNumberOfStream = events.Any() 
                    ? events.Last().SequenceNumber
                    : 0;
            }
        }

        // We need this because of LastSequenceNumber.
        // If we don't have read the stream we have no number for the Write operation
        // where we need the next sequence number.
        _domainEventStreamHasBeenRead = true;

        return events;
    }

    private SequencedDomainEvent ConvertFromEventData(JObject wrappedDomainEvent)
    {
        string eventType = wrappedDomainEvent["eventFullName"].ToString();

        Type domainEventType = Type.GetType(eventType);

        if (domainEventType == null)
        {
            throw new TypeAccessException(
                $"Deserialization Error: Event Type {eventType} is not part of Events anymore");
        }

        long sequenceNumber = wrappedDomainEvent["sequenceNumber"].Value<long>();
        IDomainEvent domainEventInstance = wrappedDomainEvent["payload"].ToObject(domainEventType, _defaultSerializer) as IDomainEvent;

        return new SequencedDomainEvent(sequenceNumber, domainEventInstance);
    }

    /// <summary>
    /// Get the next Sequence Number of Stream. A Stream have different identifier.
    /// Its always a combination of Context, Entity and EntityID. Entity and EntityID can be null or empty.
    /// So a Stream can be a Context-Stream, a Context-Entity-Stream or a Context-Entity-EntityID-Stream. 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entity"></param>
    /// <param name="entityId"></param>
    /// <returns></returns>
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
        long lastSequenceNumber = 0;

        QueryDefinition queryDefinition = new QueryDefinition(BuildQueryStringForLastSequenceNumber(context, entity, entityId))
            .WithParameter("@context", context.ToLower());

        if (string.IsNullOrWhiteSpace(entity) == false)
        {
            queryDefinition.WithParameter("@entity", entity.ToLower());
        }

        if (string.IsNullOrWhiteSpace(entityId) == false)
        {
            queryDefinition.WithParameter("@entityId", entityId.ToLower());
        }

        using (FeedIterator<LastSequenceNumberQueryResult> resultSet =
               _domainEventsContainer.GetItemQueryIterator<LastSequenceNumberQueryResult>(queryDefinition))
        {
            while (resultSet.HasMoreResults)
            {
                FeedResponse<LastSequenceNumberQueryResult> scalarResult = await resultSet.ReadNextAsync();

                lastSequenceNumber = scalarResult.Count == 1 ? scalarResult.First().MaxSequenceNumber : 0;
            }
        }

        return lastSequenceNumber;
    }

    private string BuildQueryStringForLastSequenceNumber(string context, string entity, string entityId)
    {
        string queryString = $"SELECT MAX(c.sequenceNumber) AS maxSequenceNumber FROM c "
                             + "WHERE c.context = @context";

        if (string.IsNullOrWhiteSpace(entity) == false)
        {
            queryString += " AND c.entity = @entity";
        }

        if (string.IsNullOrWhiteSpace(entityId) == false)
        {
            queryString += " AND c.entityId = @entityId";
        }

        queryString += " ORDER BY c.isoTimeStamp";

        return queryString;
    }
}