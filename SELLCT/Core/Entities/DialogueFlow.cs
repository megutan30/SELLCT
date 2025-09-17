using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 完全な対話フロー（開始条件から終了まで）を表す
    /// 複数の対話ノードを組み合わせて一連の会話シーケンスを構成
    /// トリガー条件によって開始され、ノード間の遷移で進行する
    /// パズルシステムと連携してゲーム内の対話を管理
    /// </summary>
    public class DialogueFlow
    {
        /// <summary>
        /// フローの一意識別子
        /// 対話フローを識別するためのユニークID
        /// DialogueServiceで管理・実行時の検索に使用
        /// 例: "TextWindow_Create_Flow", "No_Create_Flow"
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// フローの説明
        /// 開発者向けの説明文。この対話フローの目的や内容を記述
        /// デバッグやメンテナンス時の理解を助ける
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// このフローを開始するトリガー
        /// ファイルシステムイベント（作成、削除、存在等）に基づく開始条件
        /// ComponentManagerからのイベント通知でフローが起動される
        /// </summary>
        public DialogueTrigger Trigger { get; set; }
        
        /// <summary>
        /// このフローに含まれる全ノード
        /// ノードID（文字列）をキーとした対話ノードの辞書
        /// フロー内でのノード間遷移を管理
        /// 各ノードは個別の対話内容と次の遷移先を持つ
        /// </summary>
        public Dictionary<string, DialogueNode> Nodes { get; set; }
        
        /// <summary>
        /// 開始ノードのID
        /// フロー開始時に最初に実行されるノードの識別子
        /// 存在しないIDを指定するとフローは実行されない
        /// </summary>
        public string StartNodeId { get; set; }
        
        /// <summary>
        /// フロー実行条件（すべて満たされた場合のみ実行）
        /// トリガーが発火しても、この条件がすべて満たされていない場合は実行されない
        /// ゲーム状態、アクション回数等の詳細な条件を設定可能
        /// 空リストの場合は常に実行される
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// 繰り返し実行可能かどうか
        /// true: 条件が満たされる度に何度でも実行される
        /// false: 一度実行されると完了済みとなり、二度と実行されない
        /// 重要な対話やチュートリアルでfalseを使用
        /// </summary>
        public bool CanRepeat { get; set; }
        
        /// <summary>
        /// 優先度（数値が大きいほど優先）
        /// 複数のフローが同時にトリガーされた場合の実行順序を決定
        /// 高優先度のフローが先に実行される
        /// 同じ優先度の場合は定義順で実行
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// フローが完了済みかどうか（CanRepeat=falseの場合のみ使用）
        /// 繰り返し不可フローの実行状態を管理
        /// true: 既に実行済み、false: 未実行
        /// CanRepeat=trueの場合は常にfalseのまま
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// 対話フローの初期値を設定
        /// 安全なデフォルト値でインスタンスを初期化
        /// </summary>
        public DialogueFlow()
        {
            // ノード辞書を空で初期化
            Nodes = new Dictionary<string, DialogueNode>();
            // 実行条件リストを空で初期化
            Conditions = new List<PuzzleCondition>();
            // デフォルトでは繰り返し実行不可（安全性重視）
            CanRepeat = false;
            // デフォルト優先度は最低
            Priority = 0;
            // 未完了状態で初期化
            IsCompleted = false;
        }
        
        /// <summary>
        /// 指定されたIDのノードを取得
        /// 安全にノード辞書からノードを検索し、存在しない場合はnullを返す
        /// フロー実行時のノード遷移で使用される
        /// </summary>
        /// <param name="nodeId">取得したいノードのID</param>
        /// <returns>対応するDialogueNode、存在しない場合はnull</returns>
        public DialogueNode GetNode(string nodeId)
        {
            // TryGetValueで安全にノードを取得、存在しない場合はnullを返す
            return Nodes.TryGetValue(nodeId, out var node) ? node : null;
        }
        
        /// <summary>
        /// ノードを追加
        /// 新しい対話ノードをフローに追加
        /// 同じIDのノードが既に存在する場合は上書きされる
        /// </summary>
        /// <param name="node">追加する対話ノード</param>
        public void AddNode(DialogueNode node)
        {
            // ノードIDをキーとして辞書に格納、既存の場合は上書き
            Nodes[node.Id] = node;
        }
    }
}