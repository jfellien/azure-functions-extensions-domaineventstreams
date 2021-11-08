using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages;

namespace Azure.Functions.Extensions.DomainEventStreams.Extensions
{
    public class GetManyDomainEventFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public GetManyDomainEventFilter(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }
        /// <summary>
        /// Wheres the specified filter expression. Returns an IEnumerable of T.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <returns></returns>
        public IEnumerable<TSource> Where(Func<TSource, bool> filterExpression)
        {
            return _sourceStream.OfType<TSource>().Where(filterExpression);
        }

        public bool Any(Func<TSource, bool> filterExpression)
        {
            return _sourceStream.OfType<TSource>().Any(filterExpression);
        }
    }
}