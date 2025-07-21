using SELLCT.Core.Entities;

namespace SELLCT.Core.Events
{
    public class ComponentRenamedEvent
    {
        public string OldName { get; }
        public string NewName { get; }
        public GameComponent Component { get; }

        public ComponentRenamedEvent(string oldName, string newName, GameComponent component)
        {
            OldName = oldName;
            NewName = newName;
            Component = component;
        }
    }
}