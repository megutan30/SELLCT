using SELLCT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SELLCT.Infrastructure.Services
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<Type, List<object>> _handlers = new Dictionary<Type, List<object>>();

        public EventDispatcher()
        {
        }

        public void Dispatch<TEvent>(TEvent @event)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                foreach (var handler in handlers.Cast<Action<TEvent>>().ToList())
                {
                    handler(@event);
                }
            }
            Console.WriteLine($"Event Dispatched: {@event.GetType().Name}");
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<object>();
            }
            _handlers[eventType].Add(handler);
        }
    }
}