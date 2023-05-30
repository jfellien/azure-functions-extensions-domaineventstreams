using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventsPublisher;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing;

/// <summary>
/// Represents an Stream of Events filtered by given parameters context, entity name and entity id
/// </summary>
public class DomainEventStream : IDomainEventStream
{
    private readonly string _context;
    private readonly string _entity;
    private readonly string _entityId;
    private readonly IReadAndWriteDomainEvents _storage;
    private readonly IPublishDomainEvents _publisher;

    private DomainEventSequence _historySequence;

    /// <summary>
    /// Creates an instance with the given parameters
    /// </summary>
    /// <param name="context">Domain Context</param>
    /// <param name="entity">Name of the Entity which is represented in the event stream.</param>
    /// <param name="entityId">ID of the Entity which is represented in the event stream.</param>
    /// <param name="storage">Event Store instance</param>
    /// <param name="publisher">Event Handler publisher</param>
    public DomainEventStream(
        string context, string entity, string entityId, 
        IReadAndWriteDomainEvents storage, 
        IPublishDomainEvents publisher)
    {
        _context = context;
        _entity = entity;
        _entityId = entityId;
        _storage = storage;
        _publisher = publisher;
            
        _historySequence = new DomainEventSequence();
    }

    /// <summary>
    /// Adds an Event to the current stream
    /// </summary>
    /// <param name="domainEvent">Domain Event instance</param>
    /// <returns></returns>
    public Task Append(IDomainEvent domainEvent)
    {
        return Append(new List<IDomainEvent>
        {
            domainEvent
        });
    }
    
    /// <summary>
    /// Adds an Event to the current stream and assigns it to a specific entity.
    /// </summary>
    /// <param name="domainEvent">Domain Event instance</param>
    /// <param name="entityId">Id of Entity</param>
    /// <returns></returns>
    public Task Append(IDomainEvent domainEvent, string entityId)
    {
        return Append(new List<IDomainEvent>
        {
            domainEvent
        }, entityId);
    }

    /// <summary>
    /// Added a list of events to the current stream
    /// </summary>
    /// <param name="domainEvents">List of events</param>
    /// <returns></returns>
    public Task Append(IEnumerable<IDomainEvent> domainEvents)
    {
        return WriteToStorageAndLocalHistoryAndPublish(domainEvents, _context, _entity, _entityId);
    }
    
    /// <summary>
    /// Adds a list of events and assigns it to a specific entity
    /// </summary>
    /// <param name="domainEvents">List of events</param>
    /// <param name="entityId">Id of Entity</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If entityId is null or empty</exception>
    /// <exception cref="ArgumentException">If the entityId is different to its id used in the stream already.</exception>
    public Task Append(IEnumerable<IDomainEvent> domainEvents, string entityId)
    {
        if (string.IsNullOrWhiteSpace(entityId))
        {
            throw new ArgumentNullException(entityId);
        }
            
        if (string.IsNullOrWhiteSpace(_entityId) == false 
            && string.IsNullOrWhiteSpace(entityId) == false
            && _entityId.ToLowerInvariant() != entityId.ToLowerInvariant())
        {
            throw new ArgumentException("You have set entityId but this instance of EventStream already have, but different, an EntityId");
        }
            
        return WriteToStorageAndLocalHistoryAndPublish(domainEvents, _context, _entity, entityId);
    }

    /// <summary>
    /// Gets the event stream as instance
    /// </summary>
    /// <returns>Event Stream</returns>
    public async Task<IEnumerable<IDomainEvent>> Events()
    {
        if (_historySequence.HasBeenSequenced)
        {
            return _historySequence.Select(x => x.Instance);
        }

        DomainEventSequence storedSequence = await GetFromStorageByGivenParameters();
            
        if (_historySequence.Any())
        {
            List<SequencedDomainEvent> currentSequence = _historySequence.ToList();
            _historySequence.Clear();

            _historySequence.AddRange(storedSequence);
            _historySequence.AddRange(currentSequence);
        }
        else
        {
            _historySequence = storedSequence;
        }

        return _historySequence.Select(x => x.Instance);
    }
        
    private async Task<DomainEventSequence> GetFromStorageByGivenParameters()
    {
        DomainEventSequence domainEventSequence = null;
            
        if (string.IsNullOrWhiteSpace(_entity)
            && string.IsNullOrWhiteSpace(_entityId))
        {
            domainEventSequence = await _storage.ReadBy(_context, default);
        }

        if (string.IsNullOrWhiteSpace(_entity) == false
            && string.IsNullOrWhiteSpace(_entityId))
        {
            domainEventSequence = await _storage.ReadBy(_context, _entity, default);
        }

        if (string.IsNullOrWhiteSpace(_entity) == false
            && string.IsNullOrWhiteSpace(_entityId) == false)
        {
            domainEventSequence = await _storage.ReadBy(_context, _entity, _entityId, default);
        }

        return domainEventSequence ?? new DomainEventSequence();
    }

    private async Task WriteToStorageAndLocalHistoryAndPublish(
        IEnumerable<IDomainEvent> domainEvents, 
        string context, string entity, string entityId)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            long sequenceNumber = await WriteToStorage(domainEvent, context, entity, entityId);

            AddToLocalHistory(domainEvent, sequenceNumber);

            await PublishChanges(domainEvent);
        }
    }

    private async Task<long> WriteToStorage(IDomainEvent domainEvent, string context, string entity, string entityId)
    {
        return await _storage.Write(domainEvent, context, entity, entityId);
    }
        
    private void AddToLocalHistory(IDomainEvent domainEvent, long sequenceNumber)
    {
        _historySequence.Add(new SequencedDomainEvent(sequenceNumber, domainEvent));
    }
        
    private async Task PublishChanges(IDomainEvent domainEvent)
    {
        await _publisher.Publish(domainEvent);
    }
}