using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using devCrowd.CustomBindings.EventSourcing.EventsPublisher;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing
{
    public class DomainEventStream
    {
        private readonly string _context;
        private readonly string _entity;
        private readonly IReadAndWriteDomainEvents _storage;
        private readonly IPublishDomainEvents _publisher;

        private string _entityId;
        private DomainEventSequence _historySequence;

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

        public Task Append(IDomainEvent domainEvent)
        {
            return Append(new List<IDomainEvent>
            {
                domainEvent
            });
        }
        
        public Task Append(IDomainEvent domainEvent, string entityId)
        {
            return Append(new List<IDomainEvent>
            {
                domainEvent
            }, entityId);
        }

        public Task Append(IEnumerable<IDomainEvent> domainEvents)
        {
            return WriteToStorageAndLocalHistoryAndPublish(domainEvents, _context, _entity, _entityId);
        }
        
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
                throw new ArgumentException("You have set entityId but this instance of EventStream already have an EntityId");
            }
            
            return WriteToStorageAndLocalHistoryAndPublish(domainEvents, _context, _entity, entityId);
        }

        public async Task<IEnumerable<IDomainEvent>> Events()
        {
            if (_historySequence.HasBeenSequenced)
            {
                return _historySequence.Select(x => x.Instance);
            }

            var storedSequence = await GetFromStorageByGivenParameters();
            
            if (_historySequence.Any())
            {
                var currentSequence = _historySequence.ToList();
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
            foreach (var domainEvent in domainEvents)
            {
                var sequenceNumber = await WriteToStorage(domainEvent, context, entity, entityId);

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
}