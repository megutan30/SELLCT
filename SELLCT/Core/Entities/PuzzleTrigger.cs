using System;
using System.Linq;

namespace SELLCT.Core.Entities
{
    public class PuzzleTrigger
    {
        public enum TriggerType
        {
            Created,
            Deleted,
            Renamed,
            Exists,
            AuthorityFolderOpened
        }

        public TriggerType Type { get; set; }
        public string ComponentName { get; set; }
        public string[] ComponentNames { get; set; } // 複数の名前パターンに対応
        public string OldComponentName { get; set; } // For Renamed type

        /// <summary>
        /// 指定された名前がこのトリガーの対象かチェック
        /// </summary>
        public bool MatchesComponentName(string name)
        {
            if (ComponentNames != null && ComponentNames.Length > 0)
            {
                return ComponentNames.Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            return ComponentName != null && ComponentName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}