using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions;

public class DomainEventsFilter<TSource> where TSource : IDomainEvent
{
    private readonly IEnumerable<object> _sourceStream;
        
    public DomainEventsFilter(IEnumerable<object> sourceStream)
    {
        _sourceStream = sourceStream;
    }
        
    /// <summary>
    /// Wheres the specified filter expression. Returns a SingleOrDefault Value
    /// </summary>
    /// <param name="filterExpression">The filter expression.</param>
    /// <returns></returns>
    public TSource Where(Func<TSource, bool> filterExpression)
    {
        return _sourceStream.OfType<TSource>().SingleOrDefault(filterExpression);
    }

    public GetFirstSingleDomainEventFilter<TSource> First()
    {
        return new GetFirstSingleDomainEventFilter<TSource>(_sourceStream);
    }
        
    public GetLastSingleDomainEventFilter<TSource> Last()
    {
        return new GetLastSingleDomainEventFilter<TSource>(_sourceStream);
    }
        
    /// <summary>
    /// Gets the only one event of type TSource.
    /// </summary>
    /// <returns></returns>
    public TSource TheOnlyOne()
    {
        return _sourceStream.OfType<TSource>().SingleOrDefault();
    }

    public GetAnyDomainEventFilter<TSource> Any()
    {
        return new GetAnyDomainEventFilter<TSource>(_sourceStream);
    }

    /// <summary>
    /// Gets all Events of type TSource
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TSource> All()
    {
        return _sourceStream.OfType<TSource>();
    }
}