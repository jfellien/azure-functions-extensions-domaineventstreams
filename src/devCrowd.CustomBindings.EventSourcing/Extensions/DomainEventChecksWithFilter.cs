using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public class DomainEventChecksWithFilter<TSource> where TSource : IDomainEvent
{
    private readonly IEnumerable<object> _sourceStream;
    private readonly Func<TSource, bool> _filterExpression;

    public DomainEventChecksWithFilter(IEnumerable<object> sourceStream, Func<TSource,bool> filterExpression)
    {
        _sourceStream = sourceStream;
        _filterExpression = filterExpression;
    }
        
    public DomainEventHappenedLaterCheck<TEarlierEvent, TSource> HappenedLaterThan<TEarlierEvent>() where TEarlierEvent : IDomainEvent
    {
        if (_sourceStream.OfType<TSource>().Any() == false)
        {
            throw new ArgumentException(
                $"This type of event ({typeof(TSource).Name}) is not part of current events sequence.");
        }
            
        TSource laterEvent = _sourceStream
            .OfType<TSource>()
            .LastOrDefault(_filterExpression);

        if (laterEvent == null)
        {
            throw new ArgumentException($"Can't find Event of type {typeof(TSource)} that fits to given filter expression.");
        }
            
        return new DomainEventHappenedLaterCheck<TEarlierEvent, TSource>(_sourceStream, laterEvent);
    }
        
    public DomainEventHappenedEarlierCheck<TSource, TLaterEvent> HappenedEarlierThan<TLaterEvent>() where TLaterEvent : IDomainEvent
    {
        if (_sourceStream.OfType<TSource>().Any() == false)
        {
            throw new ArgumentException(
                $"This type of event ({typeof(TSource).Name}) is not part of current events sequence.");
        }
            
        TSource earlierEvent = _sourceStream
            .OfType<TSource>()
            .FirstOrDefault(_filterExpression);

        if (earlierEvent == null)
        {
            throw new ArgumentException($"Can't find Event of type {typeof(TSource)} that fits to given filter expression.");
        }
            
        return new DomainEventHappenedEarlierCheck<TSource, TLaterEvent>(_sourceStream, earlierEvent);
    }
        
    public bool Exists()
    {
        return _sourceStream.OfType<TSource>().Any(_filterExpression);
    }
}