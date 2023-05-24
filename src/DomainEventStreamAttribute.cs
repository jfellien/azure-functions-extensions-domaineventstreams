using System;
using Microsoft.Azure.WebJobs.Description;

namespace devCrowd.CustomBindings.EventSourcing
{
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public class DomainEventStreamAttribute : Attribute
    {
        /// <summary>
        /// Instantiated a DomainStream based on its parameters as filter on Event Stream
        /// </summary>
        /// <param name="contextName">Name of the Domain Context.
        /// Will be used as Topic on the Service Bus to publish the events after storing.</param>
        /// <param name="entityName">Name of the Entity which is the event stream related to.
        /// Its a filter setting for the event stream.</param>
        public DomainEventStreamAttribute(string contextName, string entityName) : this(contextName, entityName, null)
        { }
        
        /// <summary>
        /// Instantiated a DomainStream based on its parameters as filter on Event Stream
        /// </summary>
        /// <param name="contextName">Name of the Domain Context.
        /// Will be used as Topic on the Service Bus to publish the events after storing.</param>
        /// <param name="entityName">Name of the Entity which is the event stream related to.
        /// Its a filter setting for the event stream.</param>
        /// <param name="entityId">Id of the entity where are the events are related to.
        /// Its a filter setting for the event stream.</param>
        public DomainEventStreamAttribute(string contextName, string entityName = null, string entityId = null)
        {
            ContextName = contextName;
            EntityName = entityName;
            EntityId = entityId;
        }
        
        /// <summary>
        /// Name of the Domain Context. Will be used as Topic on the Service Bus to publish the events after storing.
        /// </summary>
        [AutoResolve] 
        public string ContextName { get; set; }
        
        /// <summary>
        /// Name of the Entity which is the event stream related to. Its a filter criteria for the event stream.
        /// </summary>
        [AutoResolve] 
        public string EntityName { get; set; }
        
        /// <summary>
        /// Id of the entity where are the events are related to. Its a filter criteria for the event stream.
        /// </summary>
        [AutoResolve] 
        public string EntityId { get; set; }
    }
}