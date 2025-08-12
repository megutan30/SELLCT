using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話フローの一つのノード（メッセージまたは選択肢）を表す
    /// </summary>
    public class DialogueNode
    {
        /// <summary>
        /// ノードの一意識別子
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 表示するテキスト（nullの場合は選択肢のみ）
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// このノードで表示する選択肢
        /// </summary>
        public List<DialogueChoice> Choices { get; set; }
        
        /// <summary>
        /// 次のノードID（選択肢がない場合の遷移先）
        /// </summary>
        public string NextNodeId { get; set; }
        
        /// <summary>
        /// このノードを表示する条件（nullの場合は常に表示）
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// このノードで実行するアクション（テキスト表示以外）
        /// </summary>
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// 繰り返し表示可能かどうか
        /// </summary>
        public bool CanRepeat { get; set; }
        
        /// <summary>
        /// このノードが既に表示済みかどうか
        /// </summary>
        public bool IsCompleted { get; set; }
        
        public DialogueNode()
        {
            Choices = new List<DialogueChoice>();
            Conditions = new List<PuzzleCondition>();
            Actions = new List<PuzzleAction>();
            CanRepeat = true;
            IsCompleted = false;
        }
    }
}