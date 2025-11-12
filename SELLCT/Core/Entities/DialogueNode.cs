using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話フローの一つのノード（メッセージまたは選択肢）を表す
    /// 対話システムの最小単位で、一つの発言内容と選択肢を管理
    /// テキスト表示、アクション実行、次ノードへの遷移を制御
    /// 条件判定により動的な対話分岐を実現
    /// </summary>
    public class DialogueNode
    {
        /// <summary>
        /// ノードの一意識別子
        /// フロー内でこのノードを識別するためのユニークID
        /// フロー遷移時の検索キーとして使用
        /// 例: "greeting", "choice_question", "yes_response"
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 表示するテキスト（nullの場合は選択肢のみ）
        /// ユーザーに表示される対話内容
        /// nullまたは空文字の場合はテキスト表示をスキップ
        /// 選択肢のみを表示したい場合に使用
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// このノードで表示する選択肢
        /// ユーザーが選択可能なオプションのリスト
        /// 空の場合は自動的に次のノードに遷移
        /// YES/NO選択肢や複数選択肢に対応
        /// </summary>
        public List<DialogueChoice> Choices { get; set; }
        
        /// <summary>
        /// 次のノードID（選択肢がない場合の遷移先）
        /// 選択肢が存在しない、または選択が完了した後の遷移先
        /// nullまたは空文字の場合は対話フローが終了
        /// 選択肢がある場合は選択肢の遷移先が優先される
        /// </summary>
        public string NextNodeId { get; set; }
        
        /// <summary>
        /// このノードを表示する条件（nullの場合は常に表示）
        /// このノードが実行される前に満たす必要がある条件
        /// 条件を満たさない場合はノードをスキップして次に進む
        /// 空リストの場合は常に実行される
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// このノードで実行するアクション（テキスト表示以外）
        /// テキスト表示と並行して実行される追加処理
        /// UI変更、可視性制御、システム操作等を実行可能
        /// 空リストの場合はテキスト表示のみ行われる
        /// </summary>
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// 繰り返し表示可能かどうか
        /// true: 同じノードに再度遷移した場合も表示される
        /// false: 一度表示されると完了済みとなり、スキップされる
        /// 重要なメッセージやワンタイムイベントでfalseを使用
        /// </summary>
        public bool CanRepeat { get; set; }
        
        /// <summary>
        /// このノードが既に表示済みかどうか
        /// CanRepeat=falseの場合のみ使用される状態管理フラグ
        /// true: 既に実行済み、false: 未実行
        /// CanRepeat=trueの場合は常にfalseのまま
        /// </summary>
        public bool IsCompleted { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// 対話ノードの初期値を設定
        /// 安全で使いやすいデフォルト値でインスタンスを初期化
        /// </summary>
        public DialogueNode()
        {
            // 選択肢リストを空で初期化
            Choices = new List<DialogueChoice>();
            // 表示条件リストを空で初期化
            Conditions = new List<PuzzleCondition>();
            // アクションリストを空で初期化
            Actions = new List<PuzzleAction>();
            // デフォルトでは繰り返し表示可能（汎用性重視）
            CanRepeat = true;
            // 未完了状態で初期化
            IsCompleted = false;
        }
    }
}