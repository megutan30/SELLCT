using System;

namespace SELLCT.Core.Interfaces
{
    public interface IEventDispatcher
    {
        void Dispatch<TEvent>(TEvent @event);
        void Subscribe<TEvent>(Action<TEvent> handler);
    }
}