using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話における選択肢を表す
    /// </summary>
    public class DialogueChoice
    {
        /// <summary>
        /// 選択肢のテキスト
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// この選択肢を選んだ場合の次のノードID
        /// </summary>
        public string NextNodeId { get; set; }
        
        /// <summary>
        /// この選択肢を選んだときに実行するアクション
        /// </summary>
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// この選択肢を表示する条件（nullの場合は常に表示）
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// 選択肢の種類（Yes/No/Custom）
        /// </summary>
        public ChoiceType Type { get; set; }
        
        public DialogueChoice()
        {
            Actions = new List<PuzzleAction>();
            Conditions = new List<PuzzleCondition>();
            Type = ChoiceType.Custom;
        }
    }
    
    public enum ChoiceType
    {
        Yes,
        No,
        Custom
    }
}