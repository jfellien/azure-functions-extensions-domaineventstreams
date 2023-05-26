using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public class DomainEventHappenedLaterCheck<TEarlier, TLater> 
    where TLater : IDomainEvent 
    where TEarlier : IDomainEvent 
{
    private readonly IEnumerable<object> _sourceStream;
    private readonly TLater _laterEvent;

    public DomainEventHappenedLaterCheck(IEnumerable<object> sourceStream, TLater laterEvent)
    {
        _sourceStream = sourceStream;
        _laterEvent = laterEvent;
    }

    public bool Where(Func<TEarlier, bool> filterExpression)
    {
        int indexOfLaterEvent = _sourceStream.ToList().IndexOf(_laterEvent);

        TEarlier earlierEvent = _sourceStream.OfType<TEarlier>().FirstOrDefault(filterExpression);

        if (earlierEvent == null)
        {
            throw new ArgumentException($"Can't find Event of type {typeof(TEarlier)} that fits to given filter expression.");
        }

        int indexOfEarlierEvent = _sourceStream.ToList().IndexOf(earlierEvent);

        return indexOfEarlierEvent < indexOfLaterEvent;
    }
}