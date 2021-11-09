using System;
using System.Collections.Generic;
using System.Linq;
using devCrowd.CustomBindings.EventSourcing.EventStreamStorages;

namespace devCrowd.CustomBindings.EventSourcing.Extensions
{
    public class DomainEventStreamQueries<TSource> where TSource : IDomainEvent
    {
        private readonly TSource _sourceDomainEvent;
        private readonly IEnumerable<object> _sourceStream;
        private readonly Func<TSource, bool> _sourceFilterExpression;

        public DomainEventStreamQueries(IEnumerable<object> sourceStream)
        {
            _sourceStream = sourceStream;
        }
        
        public DomainEventStreamQueries(IEnumerable<object> sourceStream, TSource sourceDomainEvent) 
            : this(sourceStream)
        {
            _sourceDomainEvent = sourceDomainEvent;
        }
        
        public DomainEventStreamQueries(IEnumerable<object> sourceStream, Func<TSource, bool> sourceFilterExpression) 
            : this(sourceStream)
        {
            _sourceFilterExpression = sourceFilterExpression;
        }
        
        public bool HappenedEarlierThan<TComparer>(TComparer comparerDomainEvent)
        {
            if (_sourceDomainEvent == null && comparerDomainEvent == null)
            {
                return false;
            }
            
            // Source Happened Later Than Comparer

            if (_sourceDomainEvent == null)
            {
                // Source is never happen, so it can't happen later
                return false;
            }

            if (comparerDomainEvent == null)
            {
                // Comparer is never happen, so Source is happen but not after Comparer
                return false;
            }

            // Both Events are happen, lets check the order

            var materializedSourceStream = _sourceStream.ToList();
            
            return materializedSourceStream.IndexOf(_sourceDomainEvent) 
                   < materializedSourceStream.IndexOf(comparerDomainEvent);
        }
        
        public bool HappenedEarlierOrNeverThan<TComparer>(TComparer comparerDomainEvent)
        {
            if (_sourceDomainEvent == null && comparerDomainEvent == null)
            {
                return false;
            }
            
            // Source Happened Later Than Comparer

            if (_sourceDomainEvent == null)
            {
                // Source is never happen
                return true;
            }

            if (comparerDomainEvent == null)
            {
                // Comparer is never happen, so Source is happen but not earlier than Comparer
                return false;
            }

            // Both Events are happen, lets check the order

            var materializedSourceStream = _sourceStream.ToList();
            
            return materializedSourceStream.IndexOf(_sourceDomainEvent) 
                   < materializedSourceStream.IndexOf(comparerDomainEvent);
        }
        
        public bool HappenedLaterThan<TComparer>(TComparer comparerDomainEvent)
        {
            if (_sourceDomainEvent == null && comparerDomainEvent == null)
            {
                return false;
            }
            
            // Source Happened Later Than Comparer

            if (_sourceDomainEvent == null)
            {
                // Source is never happen, so it can't happen later
                return false;
            }

            if (comparerDomainEvent == null)
            {
                // Comparer is never happen, so Source is happen but not after Comparer
                return false;
            }

            // Both Events are happen, lets check the order

            var materializedSourceStream = _sourceStream.ToList();
            
            return materializedSourceStream.IndexOf(_sourceDomainEvent) 
                   > materializedSourceStream.IndexOf(comparerDomainEvent);
        }
        
        public bool HappenedLaterOrNeverThan<TComparer>(TComparer comparerDomainEvent)
        {
            if (_sourceDomainEvent == null && comparerDomainEvent == null)
            {
                return false;
            }
            
            // Source Happened Later or Never Than Comparer

            if (_sourceDomainEvent == null)
            {
                // Source is never happen
                return true;
            }

            if (comparerDomainEvent == null)
            {
                // Comparer is never happen, so Source is happen but not later than Comparer
                return false;
            }

            // Both Events are happen, lets check the order

            var materializedSourceStream = _sourceStream.ToList();
            
            return materializedSourceStream.IndexOf(_sourceDomainEvent) 
                   > materializedSourceStream.IndexOf(comparerDomainEvent);
        }
    }
}