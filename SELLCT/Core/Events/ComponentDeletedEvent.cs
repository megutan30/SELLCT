using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    public class ComponentDeletedEvent
    {
        public GameComponent Component { get; }

        public ComponentDeletedEvent(GameComponent component)
        {
            Component = component;
        }
    }
}