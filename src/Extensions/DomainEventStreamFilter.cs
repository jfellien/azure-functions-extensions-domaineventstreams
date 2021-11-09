using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
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