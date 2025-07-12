using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    public class ComponentCreatedEvent
    {
        public GameComponent Component { get; }

        public ComponentCreatedEvent(GameComponent component)
        {
            Component = component;
        }
    }
}