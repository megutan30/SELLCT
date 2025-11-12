using SELLCT.Core.Entities;
using SELLCT.Core.Events;

namespace SELLCT.Core.Interfaces
{
    /// <summary>
    /// 対話サービスインターフェース
    /// 対話フロー管理システムの抽象化インターフェース
    /// Clean Architectureの依存性逆転原則に従った設計
    /// ドメイン層がアプリケーション層の実装に依存しないようにするための契約
    /// 対話フロー実行、ノード遷移、選択肢処理、イベント連携を抽象化
    /// </summary>
    public interface IDialogueService
    {
        /// <summary>
        /// 対話フローを実行開始する
        /// 指定されたIDの対話フローを開始し、指定されたノードから実行を開始
        /// フロー存在確認、条件判定、初期ノード設定を行う
        /// 実行中の他のフローがある場合は適切に処理する
        /// </summary>
        /// <param name="flowId">実行する対話フローの識別ID</param>
        /// <param name="startNodeId">開始ノードID（nullの場合はフローのデフォルト開始ノード）</param>
        void ExecuteDialogue(string flowId, string startNodeId = null);
        
        /// <summary>
        /// 次のノードに遷移する
        /// 現在のノードから指定されたノードまたは自動決定されたノードへ進む
        /// ノード存在確認、条件判定、アクション実行を行う
        /// 遷移先が存在しない場合は対話を終了する
        /// </summary>
        /// <param name="nodeId">遷移先ノードID（nullの場合は現在ノードの次ノードIDを使用）</param>
        void NextNode(string nodeId = null);
        
        /// <summary>
        /// インデックスで選択肢を選択する
        /// 現在のノードの選択肢リストから指定されたインデックスの選択肢を選択
        /// 選択肢の条件判定、アクション実行、ノード遷移を行う
        /// 無効なインデックスの場合は何もしない
        /// </summary>
        /// <param name="choiceIndex">選択する選択肢のインデックス（0から開始）</param>
        void MakeChoice(int choiceIndex);
        
        /// <summary>
        /// Yes選択肢を選択する
        /// 現在のノードの選択肢からChoiceType.Yesの選択肢を自動検索して選択
        /// Yes選択肢が存在しない場合は何もしない
        /// YES/NOコンポーネントの存在も考慮した処理を行う
        /// </summary>
        void ChooseYes();
        
        /// <summary>
        /// No選択肢を選択する
        /// 現在のノードの選択肢からChoiceType.Noの選択肢を自動検索して選択
        /// No選択肢が存在しない場合は何もしない
        /// YES/NOコンポーネントの存在も考慮した処理を行う
        /// </summary>
        void ChooseNo();
        
        /// <summary>
        /// 現在対話中かどうかを示す状態プロパティ
        /// 対話フローが実行中でノードが存在する場合にtrueを返す
        /// UI状態管理や他のシステムとの連携で使用される
        /// 読み取り専用プロパティ
        /// </summary>
        bool IsInDialogue { get; }
        
        /// <summary>
        /// 現在実行中のノード
        /// 対話中の現在のDialogueNodeインスタンス
        /// 対話中でない場合はnullを返す
        /// UI表示やデバッグで使用される
        /// 読み取り専用プロパティ
        /// </summary>
        DialogueNode CurrentNode { get; }
        
        /// <summary>
        /// 対話状態をリセットする
        /// 現在実行中の対話フローを強制終了し、状態を初期化
        /// 緊急時やゲーム状態リセット時に使用される
        /// すべての対話関連データをクリアする
        /// </summary>
        void ResetDialogue();
        
        /// <summary>
        /// コンポーネント作成イベントに基づく対話トリガーチェック
        /// ファイル作成イベントを受け取り、該当する対話フローの開始条件を判定
        /// トリガー条件が満たされた場合は自動的に対話フローを開始
        /// </summary>
        /// <param name="event">コンポーネント作成イベント</param>
        void CheckDialogueTriggersOnComponentCreated(ComponentCreatedEvent @event);
        
        /// <summary>
        /// コンポーネント削除イベントに基づく対話トリガーチェック
        /// ファイル削除イベントを受け取り、該当する対話フローの開始条件を判定
        /// トリガー条件が満たされた場合は自動的に対話フローを開始
        /// </summary>
        /// <param name="event">コンポーネント削除イベント</param>
        void CheckDialogueTriggersOnComponentDeleted(ComponentDeletedEvent @event);
        
        /// <summary>
        /// コンポーネント名前変更イベントに基づく対話トリガーチェック
        /// ファイル名前変更イベントを受け取り、該当する対話フローの開始条件を判定
        /// トリガー条件が満たされた場合は自動的に対話フローを開始
        /// </summary>
        /// <param name="event">コンポーネント名前変更イベント</param>
        void CheckDialogueTriggersOnComponentRenamed(ComponentRenamedEvent @event);
    }
}