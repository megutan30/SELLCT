using System.Collections.Generic;

namespace SELLCT.Core.Entities
{
    /// <summary>
    /// パズル定義クラス
    /// ゲーム内の個別のパズルとその実行条件、アクションを定義する
    /// ファイルシステムのイベントに対してどのような反応をするかを設定
    /// パズルシステムの基本単位となるクラス
    /// </summary>
    public class PuzzleDefinition
    {
        /// <summary>
        /// パズル識別子
        /// パズルを一意に識別するためのID
        /// 例: "Button_Delete_First", "TextWindow_Created"
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// パズルの説明
        /// 開発者向けの説明文。このパズルが何をするかを記述
        /// デバッグやメンテナンス時に参照される
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// パズルトリガー
        /// このパズルが実行される条件（ファイル作成、削除、名前変更など）
        /// ファイルシステムの特定のイベントにマッチした時にパズルが実行される
        /// </summary>
        public PuzzleTrigger Trigger { get; set; }
        
        /// <summary>
        /// 実行アクションリスト
        /// パズルがトリガーされた時に実行される具体的なアクション群
        /// UI変更、ダイアログ表示、システム制御などの処理を順次実行
        /// </summary>
        public List<PuzzleAction> Actions { get; set; }
        
        /// <summary>
        /// パズル実行条件（すべて満たされた場合のみ実行）
        /// トリガーが発火しても、この条件がすべて満たされていない場合は実行されない
        /// ゲーム状態、アクション回数、コンポーネント存在などの詳細な条件を設定可能
        /// 空リストの場合は常に実行される
        /// </summary>
        public List<PuzzleCondition> Conditions { get; set; }
        
        /// <summary>
        /// 繰り返し実行可能かどうか
        /// true: 条件が満たされる度に何度でも実行される
        /// false: 一度実行されると完了済みとなり、二度と実行されない
        /// ワンタイムイベントやチュートリアルなどで使用
        /// </summary>
        public bool CanRepeat { get; set; }
        
        /// <summary>
        /// 優先度（数値が大きいほど優先）
        /// 複数のパズルが同時にトリガーされた場合の実行順序を決定
        /// 高優先度のパズルが先に実行される
        /// 同じ優先度の場合は定義順で実行
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// パズルが完了済みかどうか（CanRepeat=falseの場合のみ使用）
        /// 繰り返し不可パズルの実行状態を管理
        /// true: 既に実行済み、false: 未実行
        /// CanRepeat=trueの場合は常にfalseのまま
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// コンストラクタ
        /// パズル定義の初期値を設定
        /// 安全なデフォルト値でインスタンスを初期化
        /// </summary>
        public PuzzleDefinition()
        {
            // 実行条件リストを空で初期化
            Conditions = new List<PuzzleCondition>();
            // デフォルトでは繰り返し実行不可（安全性重視）
            CanRepeat = false;
            // デフォルト優先度は最低
            Priority = 0;
            // 未完了状態で初期化
            IsCompleted = false;
        }
    }
}