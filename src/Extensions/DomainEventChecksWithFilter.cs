using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
    public class DomainEventChecksWithFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        private readonly Func<TSource, bool> _filterExpression;

        public DomainEventChecksWithFilter(IEnumerable<object> sourceStream, Func<TSource,bool> filterExpression)
        {
            _sourceStream = sourceStream;
            _filterExpression = filterExpression;
        }
        
        public bool Exists()
        {
            return _sourceStream.OfType<TSource>().Any(_filterExpression);
        }
    }
}