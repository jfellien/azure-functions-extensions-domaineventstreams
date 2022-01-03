using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
    public class GetAnyDomainEventFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public GetAnyDomainEventFilter(IEnumerable<object> sourceStream)
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
    }
}