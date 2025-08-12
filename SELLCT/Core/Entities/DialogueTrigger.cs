namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話フローの開始トリガーを表す
    /// </summary>
    public class DialogueTrigger
    {
        /// <summary>
        /// トリガーの種類
        /// </summary>
        public TriggerType Type { get; set; }
        
        /// <summary>
        /// 対象コンポーネント名（複数対応）
        /// </summary>
        public string[] ComponentNames { get; set; }
        
        /// <summary>
        /// リネーム時の旧名前
        /// </summary>
        public string OldComponentName { get; set; }
        
        /// <summary>
        /// 特定のコンポーネント名（リネーム時の新名前など）
        /// </summary>
        public string ComponentName { get; set; }
        
        public enum TriggerType
        {
            Created,
            Deleted, 
            Exists,
            Renamed
        }
        
        /// <summary>
        /// コンポーネント名がこのトリガーにマッチするかチェック
        /// </summary>
        public bool MatchesComponentName(string componentName)
        {
            if (ComponentNames != null)
            {
                foreach (var name in ComponentNames)
                {
                    if (string.Equals(name, componentName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            
            return string.Equals(ComponentName, componentName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}