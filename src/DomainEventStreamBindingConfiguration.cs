using System;
using devCrowd.CustomBindings.EventSourcing.EventsPublisher;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace devCrowd.CustomBindings.EventSourcing
{
    [Extension("DomainEventStream")]
    public class DomainEventStreamBindingConfiguration : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context
                .AddBindingRule<DomainEventStreamAttribute>()
                .BindToInput<DomainEventStream>(GetFromAttribute);
        }

        private DomainEventStream GetFromAttribute(DomainEventStreamAttribute attribute)
        {
            var eventStoreConnectionString = Environment.GetEnvironmentVariable("EVENT_STORE_CONNECTION_STRING");
            var eventStoreDatabaseName = Environment.GetEnvironmentVariable("EVENT_STORE_DB_NAME");
            var eventsCollectionName = Environment.GetEnvironmentVariable("DOMAIN_EVENTS_COLLECTION_NAME");
            
            if (string.IsNullOrEmpty(eventStoreConnectionString))
            {
                throw new ArgumentException("EVENT_STORE_CONNECTION_STRING not set in Application Settings");
            }
            
            var domainEventStreamStorage = DomainEventStreamStorageLibrary.GetInstanceBy(
                eventStoreConnectionString,
                eventStoreDatabaseName,
                eventsCollectionName);

            if (domainEventStreamStorage == null)
            {
                throw new ArgumentException(
                    $"Unexpected type of Connection String {eventStoreConnectionString[..15]}. Can not instantiate a DomainEventStream Storage. Please fix the Connection String or use only a Sql Server or CosmosDB Connection String.");
            }
            
            var serviceBusConnectionString = Environment.GetEnvironmentVariable("EVENT_HANDLER_CONNECTION_STRING");
            
            if (string.IsNullOrEmpty(serviceBusConnectionString))
            {
                throw new ArgumentException("EVENT_HANDLER_CONNECTION_STRING not set in Application Settings");
            }
            
            var domainEventsPublisher = new ServiceBusDomainEventsPublisher(
                serviceBusConnectionString,
                attribute.ContextName);
            
            return new DomainEventStream(
                attribute.ContextName,
                attribute.EntityName,
                attribute.EntityId,
                domainEventStreamStorage,
                domainEventsPublisher);
        }
    }
}