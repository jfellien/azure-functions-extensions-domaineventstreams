using System;
using System.Collections.Generic;
using System.Linq;
using AzureFunctions.Extensions.EventSourcing.EventStreamStorages;

namespace AzureFunctions.Extensions.EventSourcing.Extensions
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