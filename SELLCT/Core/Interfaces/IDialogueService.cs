using SELLCT.Core.Entities;
using SELLCT.Core.Events;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// 対話フロー管理サービスのインターフェース
    /// </summary>
    public interface IDialogueService
    {
        /// <summary>
        /// 対話フローを実行
        /// </summary>
        void ExecuteDialogue(string flowId, string startNodeId = null);
        
        /// <summary>
        /// 現在の対話フローで次のノードに進む
        /// </summary>
        void NextNode(string nodeId = null);
        
        /// <summary>
        /// 選択肢を選択
        /// </summary>
        void MakeChoice(int choiceIndex);
        
        /// <summary>
        /// Yes選択肢を選択
        /// </summary>
        void ChooseYes();
        
        /// <summary>
        /// No選択肢を選択
        /// </summary>
        void ChooseNo();
        
        /// <summary>
        /// 現在対話中かどうか
        /// </summary>
        bool IsInDialogue { get; }
        
        /// <summary>
        /// 現在のノード
        /// </summary>
        DialogueNode CurrentNode { get; }
        
        /// <summary>
        /// 対話フローをリセット
        /// </summary>
        void ResetDialogue();
        
        /// <summary>
        /// イベントベースの対話開始チェック
        /// </summary>
        void CheckDialogueTriggersOnComponentCreated(ComponentCreatedEvent @event);
        void CheckDialogueTriggersOnComponentDeleted(ComponentDeletedEvent @event);
        void CheckDialogueTriggersOnComponentRenamed(ComponentRenamedEvent @event);
    }
}