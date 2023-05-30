using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace devCrowd.CustomBindings.EventSourcing;

public interface IDomainEventStream
{
    /// <summary>
    /// Adds an Event to the current stream
    /// </summary>
    /// <param name="domainEvent">Domain Event instance</param>
    /// <returns></returns>
    Task Append(IDomainEvent domainEvent);

    /// <summary>
    /// Adds an Event to the current stream and assigns it to a specific entity.
    /// </summary>
    /// <param name="domainEvent">Domain Event instance</param>
    /// <param name="entityId">Id of Entity</param>
    /// <returns></returns>
    Task Append(IDomainEvent domainEvent, string entityId);

    /// <summary>
    /// Added a list of events to the current stream
    /// </summary>
    /// <param name="domainEvents">List of events</param>
    /// <returns></returns>
    Task Append(IEnumerable<IDomainEvent> domainEvents);

    /// <summary>
    /// Adds a list of events and assigns it to a specific entity
    /// </summary>
    /// <param name="domainEvents">List of events</param>
    /// <param name="entityId">Id of Entity</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If entityId is null or empty</exception>
    /// <exception cref="ArgumentException">If the entityId is different to its id used in the stream already.</exception>
    Task Append(IEnumerable<IDomainEvent> domainEvents, string entityId);

    /// <summary>
    /// Gets the event stream as instance
    /// </summary>
    /// <returns>Event Stream</returns>
    Task<IEnumerable<IDomainEvent>> Events();
}