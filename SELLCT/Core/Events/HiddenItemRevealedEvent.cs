namespace SELLCT.Core.Events
{
    public class HiddenItemRevealedEvent
    {
        public string ItemName { get; }
        public string DisplayName { get; }
        public string Folder { get; }

        public HiddenItemRevealedEvent(string itemName, string displayName, string folder)
        {
            ItemName = itemName;
            DisplayName = displayName;
            Folder = folder;
        }
    }
}