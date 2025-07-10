namespace SELLCT.Models
{
    public class PuzzleTrigger
    {
        public enum TriggerType
        {
            Created,
            Deleted,
            Renamed,
            Exists // 新しいトリガータイプ
        }

        public TriggerType Type { get; set; }
        public string ComponentName { get; set; }
        public string OldComponentName { get; set; } // For Renamed type
    }
}