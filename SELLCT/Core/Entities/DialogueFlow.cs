using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 完全な対話フロー（開始条件から終了まで）を表す
    /// </summary>
    public class DialogueFlow
    {
        /// <summary>
        /// フローの一意識別子
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// フローの説明
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// このフローを開始するトリガー
        /// </summary>
        public DialogueTrigger Trigger { get; set; }
        
        /// <summary>
        /// このフローに含まれる全ノード
        /// </summary>
        public Dictionary<string, DialogueNode> Nodes { get; set; }
        
        /// <summary>
        /// 開始ノードのID
        /// </summary>
        public string StartNodeId { get; set; }
        
        /// <summary>
        /// フロー実行条件（すべて満たされた場合のみ実行）
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// 繰り返し実行可能かどうか
        /// </summary>
        public bool CanRepeat { get; set; }
        
        /// <summary>
        /// 優先度（数値が大きいほど優先）
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// フローが完了済みかどうか（CanRepeat=falseの場合のみ使用）
        /// </summary>
        public bool IsCompleted { get; set; }
        
        public DialogueFlow()
        {
            Nodes = new Dictionary<string, DialogueNode>();
            Conditions = new List<PuzzleCondition>();
            CanRepeat = false;
            Priority = 0;
            IsCompleted = false;
        }
        
        /// <summary>
        /// 指定されたIDのノードを取得
        /// </summary>
        public DialogueNode GetNode(string nodeId)
        {
            return Nodes.TryGetValue(nodeId, out var node) ? node : null;
        }
        
        /// <summary>
        /// ノードを追加
        /// </summary>
        public void AddNode(DialogueNode node)
        {
            Nodes[node.Id] = node;
        }
    }
}