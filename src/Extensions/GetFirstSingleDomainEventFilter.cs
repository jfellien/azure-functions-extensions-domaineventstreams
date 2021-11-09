using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
    public class GetFirstSingleDomainEventFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public GetFirstSingleDomainEventFilter(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }
        /// <summary>
        /// Wheres the specified filter expression. Returns a FirstOrDefaul Value.
        /// </summary>
        /// <param name="filterExpression">The filter expression.</param>
        /// <returns></returns>
        public TSource Where(Func<TSource, bool> filterExpression)
        {
            return _sourceStream.OfType<TSource>().FirstOrDefault(filterExpression);
        }
    }
}