using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
    public class GetSingleDomainEventFilter<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;
        public GetSingleDomainEventFilter(IEnumerable<object> sourceStream)
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
        
        /// <summary>
        /// Gets a single event of given type. If no event given it gets an default(TSource).
        /// </summary>
        /// <returns></returns>
        public TSource Single()
        {
            return _sourceStream.OfType<TSource>().SingleOrDefault();
        }
    }
}