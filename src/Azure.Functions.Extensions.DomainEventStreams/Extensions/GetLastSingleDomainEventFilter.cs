using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages;

namespace Azure.Functions.Extensions.DomainEventStreams.Extensions
{
    public class GetLastSingleDomainEventFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public GetLastSingleDomainEventFilter(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }
        /// <summary>
        /// Wheres the specified filter expression. Returns LastOrDefault value.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <returns></returns>
        public TSource Where(Func<TSource, bool> filterExpression)
        {
            return _sourceStream.OfType<TSource>().LastOrDefault(filterExpression);
        }
    }
}