namespace SELLCT.Core.Entities
{
    public class PuzzleTrigger
    {
        public enum TriggerType
        {
            Created,
            Deleted,
            Renamed,
            Exists
        }

        public TriggerType Type { get; set; }
        public string ComponentName { get; set; }
        public string OldComponentName { get; set; } // For Renamed type
    }
}