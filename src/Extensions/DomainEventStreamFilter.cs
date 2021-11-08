using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;

namespace AzureFunctions.Extensions.EventSourcing.Extensions
{
    public class DomainEventStreamFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public DomainEventStreamFilter(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }

        public DomainEventStreamQueries<TSource> Where(Func<TSource, bool> filterExpression)
        {
            return new DomainEventStreamQueries<TSource>(_sourceStream, filterExpression);
        }

        public TSource Get(Func<TSource, bool> filterExpression)
        {
            return _sourceStream.OfType<TSource>().SingleOrDefault(filterExpression);
        }
    }
}