using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public class DomainEventHappenedEarlierCheck<TEarlier, TLater> 
    where TLater : IDomainEvent 
    where TEarlier : IDomainEvent 
{
    private readonly IEnumerable<object> _sourceStream;
    private readonly TEarlier _earlierEvent;

    public DomainEventHappenedEarlierCheck(IEnumerable<object> sourceStream, TEarlier earlierEvent)
    {
        _sourceStream = sourceStream;
        _earlierEvent = earlierEvent;
    }

    public bool Where(Func<TLater, bool> filterExpression)
    {
        int indexOfEarlierEvent = _sourceStream.ToList().IndexOf(_earlierEvent);

        TLater laterEvent = _sourceStream.OfType<TLater>().LastOrDefault(filterExpression);

        if (laterEvent == null)
        {
            throw new ArgumentException($"Can't find Event of type {typeof(TLater)} that fits to given filter expression.");
        }

        int indexOfLaterEvent = _sourceStream.ToList().IndexOf(laterEvent);

        return indexOfLaterEvent > indexOfEarlierEvent;
    }
}