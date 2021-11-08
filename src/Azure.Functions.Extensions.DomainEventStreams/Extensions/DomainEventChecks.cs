using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Functions.Extensions.DomainEventStreams.EventStreamStorages;

namespace Azure.Functions.Extensions.DomainEventStreams.Extensions
{
    public class DomainEventChecks<TSource> where TSource : IDomainEvent
    {
        private readonly IEnumerable<object> _sourceStream;

        public DomainEventChecks(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }
        
        /// <summary>
        /// Gets an specific event by filter criterias
        /// </summary>
        /// <param name="filterExpression"></param>
        /// <returns></returns>
        public DomainEventChecksWithFilter<TSource> Where(Func<TSource, bool> filterExpression)
        {
            return new DomainEventChecksWithFilter<TSource>(_sourceStream, filterExpression);
        }
        
        /// <summary>
        /// Checks if an specific type of event exists later than compared type
        /// </summary>
        /// <typeparam name="TComparer"></typeparam>
        /// <returns></returns>
        public bool HappenedEarlierThan<TComparer>()
        {
            var sourceEventFound = false;
            var comparsionEventFound = false;
            
            foreach (var domainEvent in _sourceStream)
            {
                if (domainEvent is TSource)
                {
                    sourceEventFound = true;
                }

                if (domainEvent is TComparer)
                {
                    comparsionEventFound = true;
                }

                if (sourceEventFound && comparsionEventFound == false)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Checks if a specific type of event exists earlier then compared type
        /// </summary>
        /// <typeparam name="TComparer"></typeparam>
        /// <returns></returns>
        public bool HappenedLaterThan<TComparer>()
        {
            var sourceEventFound = false;
            var comparsionEventFound = false;
            
            foreach (var domainEvent in _sourceStream.Reverse())
            {
                if (domainEvent is TSource)
                {
                    sourceEventFound = true;
                }

                if (domainEvent is TComparer)
                {
                    comparsionEventFound = true;
                }

                if (sourceEventFound && comparsionEventFound == false)
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Checks if an Event of a specific type exists
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            return _sourceStream.OfType<TSource>().Any();
        }
    }
}