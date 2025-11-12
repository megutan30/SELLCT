using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// 対話選択肢クラス
    /// DialogueNode内で提示される個々の選択オプションを表す
    /// ユーザーが選択可能な対話の分岐点を管理し、選択結果に応じた処理を実行
    /// 条件付き表示、アクション実行、ノード遷移の機能を持つ
    /// </summary>
    public class DialogueChoice
    {
        /// <summary>
        /// 選択肢の表示テキスト
        /// ユーザーに表示される選択肢の内容
        /// ボタンやリストアイテムとして画面に表示される
        /// 例: "はい", "いいえ", "詳細を確認する", "キャンセル"
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// 選択時の遷移先ノードID
        /// この選択肢が選ばれた場合に移動する次の対話ノードの識別子
        /// nullまたは空文字の場合は対話フローが終了する
        /// Actionsが設定されている場合は、アクション実行後に遷移
        /// </summary>
        public string NextNodeId { get; set; }
        
        /// <summary>
        /// 選択時実行アクションリスト
        /// この選択肢が選ばれた時に実行される処理の配列
        /// UI変更、システム操作、ゲーム状態更新等を実行可能
        /// 空リストの場合は単純なノード遷移のみ行われる
        /// アクション実行後にNextNodeIdへ遷移する
        /// </summary>
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// 選択肢表示条件リスト
        /// この選択肢を表示するために満たす必要がある条件群
        /// すべての条件が満たされた場合のみ選択肢として表示される
        /// 空リストの場合は常に表示される
        /// ゲーム進行度やコンポーネント状態による動的な選択肢制御に使用
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// 選択肢の種類
        /// 選択肢の特性や表示方法を決定する分類
        /// Yes/No選択肢では特別な処理や表示が適用される場合がある
        /// Customは汎用的な選択肢として扱われる
        /// </summary>
        public ChoiceType Type { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// 対話選択肢の初期値を設定
        /// 安全で使いやすいデフォルト値でインスタンスを初期化
        /// </summary>
        public DialogueChoice()
        {
            // アクションリストを空で初期化
            Actions = new List<PuzzleAction>();
            // 表示条件リストを空で初期化
            Conditions = new List<PuzzleCondition>();
            // デフォルトではカスタム選択肢として設定
            Type = ChoiceType.Custom;
        }
    }
    
    /// <summary>
    /// 選択肢種類列挙型
    /// 対話選択肢の性質や用途による分類を定義
    /// 選択肢の表示方法や処理方法を決定するために使用
    /// </summary>
    public enum ChoiceType
    {
        /// <summary>
        /// 肯定選択肢（Yes）
        /// ユーザーの同意や承認を表す選択肢
        /// 「はい」「OK」「実行する」等の肯定的な回答
        /// 特別なスタイリングや優先表示が適用される場合がある
        /// </summary>
        Yes,
        
        /// <summary>
        /// 否定選択肢（No）
        /// ユーザーの拒否や否認を表す選択肢
        /// 「いいえ」「キャンセル」「実行しない」等の否定的な回答
        /// 安全性を重視した表示や確認処理が適用される場合がある
        /// </summary>
        No,
        
        /// <summary>
        /// カスタム選択肢（Custom）
        /// Yes/No以外の任意の選択肢
        /// 汎用的な対話選択として使用される
        /// 詳細確認、追加オプション、複数選択肢等で使用
        /// </summary>
        Custom
    }
}