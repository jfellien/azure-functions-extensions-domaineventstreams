using System;
using devCrowd.CustomBindings.EventSourcing.EventsPublisher;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace devCrowd.CustomBindings.EventSourcing;

[Extension("DomainEventStream")]
public class DomainEventStreamBindingConfiguration : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        context
            .AddBindingRule<DomainEventStreamAttribute>()
            .BindToInput(GetFromAttribute);
    }

    private static DomainEventStream GetFromAttribute(DomainEventStreamAttribute attribute)
    {
        string eventStoreConnection = GetEventStoreConnectionFromEnvVars();
        string eventStoreDatabaseName = Environment.GetEnvironmentVariable("EVENT_STORE_DB_NAME");
        string eventsCollectionName = Environment.GetEnvironmentVariable("DOMAIN_EVENTS_COLLECTION_NAME");
            
        if (string.IsNullOrEmpty(eventStoreConnection))
        {
            throw new ArgumentException("EVENT_STORE_CONNECTION_STRING or EVENT_STORE__accountEndpoint not set in Application Settings");
        }

        if (string.IsNullOrEmpty("EVENT_STORE_DB_NAME"))
        {
            throw new ArgumentException("EVENT_STORE_DB_NAME not set in Application Settings");
        }
        
        if (string.IsNullOrEmpty("DOMAIN_EVENTS_COLLECTION_NAME"))
        {
            throw new ArgumentException("DOMAIN_EVENTS_COLLECTION_NAME not set in Application Settings");
        }
            
        IReadAndWriteDomainEvents domainEventStreamStorage = DomainEventStreamStorageLibrary.GetInstanceBy(
            eventStoreConnection,
            eventStoreDatabaseName,
            eventsCollectionName);

        if (domainEventStreamStorage == null)
        {
            throw new ArgumentException(
                $"Unexpected type of Connection String (starts with: '{eventStoreConnection[..15]}'). Can not instantiate a DomainEventStream Storage. Please fix the Connection String or use only a Sql Server or CosmosDB Connection String.");
        }

        string serviceBusConnection = GetEventHandlerConnectionFromEnvVars();
            
        if (string.IsNullOrEmpty(serviceBusConnection))
        {
            throw new ArgumentException("EVENT_HANDLER_CONNECTION_STRING or EVENT_HANDLER__fullyQualifiedNameSpace not set in Application Settings");
        }
            
        ServiceBusDomainEventsPublisher domainEventsPublisher = new (
            serviceBusConnection,
            attribute.ContextName);
            
        return new DomainEventStream(
            attribute.ContextName,
            attribute.EntityName,
            attribute.EntityId,
            domainEventStreamStorage,
            domainEventsPublisher);
    }

    private static string GetEventStoreConnectionFromEnvVars()
    {
        return Environment.GetEnvironmentVariable("EVENT_STORE_CONNECTION_STRING") 
               ?? Environment.GetEnvironmentVariable("EVENT_STORE__accountEndpoint");
        
    }
    
    private static string GetEventHandlerConnectionFromEnvVars()
    {
        return Environment.GetEnvironmentVariable("EVENT_HANDLER_CONNECTION_STRING") 
               ?? Environment.GetEnvironmentVariable("EVENT_HANDLER__fullyQualifiedNameSpace");
        
    }
}