using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    public class PuzzleDefinition
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public PuzzleTrigger Trigger { get; set; }
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// パズル実行条件（すべて満たされた場合のみ実行）
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
        /// パズルが完了済みかどうか（CanRepeat=falseの場合のみ使用）
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PuzzleDefinition()
        {
            Conditions = new List<PuzzleCondition>();
            CanRepeat = false;
            Priority = 0;
            IsCompleted = false;
        }
    }
}